using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Pong.Core;
using Pong.Network;

namespace Pong.Server
{
    /// <summary>
    /// Main server class that manages game sessions and player connections
    /// </summary>
    public class GameServer : IDisposable
    {
        private readonly TcpListener tcpListener;
        private readonly NetworkManager networkManager;
        private readonly GameState gameState;
        private readonly GameLogic gameLogic;
        private readonly List<PlayerConnection> playerConnections;
        
        private CancellationTokenSource cancellationTokenSource;
        private bool isGameRunning = false;
        private bool isServerRunning = false;
        private readonly object gameStateLock = new object();

        public GameServer()
        {
            tcpListener = new TcpListener(IPAddress.Any, GameConfiguration.ServerPort);
            networkManager = new NetworkManager();
            gameState = new GameState();
            gameLogic = new GameLogic(gameState);
            playerConnections = new List<PlayerConnection>();
        }

        /// <summary>
        /// Starts the server and begins accepting player connections
        /// </summary>
        public async Task StartAsync()
        {
            if (isServerRunning)
                return;

            tcpListener.Start();
            isServerRunning = true;
            cancellationTokenSource = new CancellationTokenSource();

            System.Console.WriteLine("Pong server started. Waiting for players to connect...");
            
            await AcceptPlayersAsync(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the server and disconnects all players
        /// </summary>
        public void Stop()
        {
            if (!isServerRunning)
                return;

            try
            {
                cancellationTokenSource?.Cancel();
                isGameRunning = false;
                isServerRunning = false;
                
                tcpListener.Stop();
                networkManager.DisconnectAllClients();
                playerConnections.Clear();

                System.Console.WriteLine("Server stopped successfully.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error stopping server: {ex.Message}");
            }
        }

        private async Task AcceptPlayersAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await WaitForPlayersAsync(cancellationToken);
                    
                    if (playerConnections.Count < 2)
                        continue;

                    await StartGameSessionAsync(cancellationToken);
                    await Task.Delay(GameConfiguration.ReconnectionDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error in player acceptance loop: {ex.Message}");
            }
        }

        private async Task WaitForPlayersAsync(CancellationToken cancellationToken)
        {
            while (playerConnections.Count < 2 && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    var playerId = playerConnections.Count + 1;
                    
                    var playerConnection = new PlayerConnection(tcpClient, playerId);
                    playerConnections.Add(playerConnection);
                    networkManager.RegisterClient(tcpClient);

                    System.Console.WriteLine($"Player {playerId} connected from {tcpClient.Client.RemoteEndPoint}");
                    
                    _ = Task.Run(() => HandlePlayerAsync(playerConnection, cancellationToken), cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    // Server was stopped
                    break;
                }
            }
        }

        private async Task HandlePlayerAsync(PlayerConnection player, CancellationToken cancellationToken)
        {
            try
            {
                await networkManager.SendMessageToClientAsync(player.TcpClient, $"ASSIGN:{player.PlayerId}");
                
                using var reader = new System.IO.StreamReader(player.TcpClient.GetStream(), System.Text.Encoding.UTF8);
                
                while (player.TcpClient.Connected && !cancellationToken.IsCancellationRequested)
                {
                    string command = await networkManager.ReadLineAsync(reader);
                    
                    if (command == null)
                        break;

                    await ProcessPlayerCommand(player.PlayerId, command);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error handling player {player.PlayerId}: {ex.Message}");
            }
            finally
            {
                await HandlePlayerDisconnection(player);
            }
        }

        private async Task ProcessPlayerCommand(int playerId, string command)
        {
            if (!command.StartsWith("MOVE:"))
                return;

            if (!int.TryParse(command.Substring(5), out int direction))
                return;

            lock (gameStateLock)
            {
                gameLogic.MovePaddle(playerId, direction);
            }
        }

        private async Task HandlePlayerDisconnection(PlayerConnection player)
        {
            System.Console.WriteLine($"Player {player.PlayerId} disconnected");
            
            playerConnections.Remove(player);
            networkManager.UnregisterClient(player.TcpClient);
            
            await networkManager.BroadcastMessageAsync("QUIT");
            ResetGameSession();
        }

        private async Task StartGameSessionAsync(CancellationToken cancellationToken)
        {
            await ExecuteCountdownAsync();
            await networkManager.BroadcastMessageAsync("START");
            
            isGameRunning = true;
            await RunGameLoopAsync(cancellationToken);
        }

        private async Task ExecuteCountdownAsync()
        {
            for (int countdown = 3; countdown >= 1; countdown--)
            {
                await networkManager.BroadcastMessageAsync($"COUNTDOWN:{countdown}");
                await Task.Delay(GameConfiguration.CountdownDuration);
            }
        }

        private async Task RunGameLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (isGameRunning && !cancellationToken.IsCancellationRequested)
                {
                    string stateData;
                    lock (gameStateLock)
                    {
                        gameLogic.UpdateGame();
                        stateData = $"STATE:{gameState.SerializeState()}";
                    }

                    await networkManager.BroadcastMessageAsync(stateData);
                    await Task.Delay(GameConfiguration.GameLoopDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        private void ResetGameSession()
        {
            isGameRunning = false;
            lock (gameStateLock)
            {
                gameState.Reset();
            }
            System.Console.WriteLine("Game session reset. Waiting for new players...");
        }

        public void Dispose()
        {
            Stop();
            networkManager?.Dispose();
            cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// Represents a connected player
    /// </summary>
    internal class PlayerConnection
    {
        public TcpClient TcpClient { get; }
        public int PlayerId { get; }

        public PlayerConnection(TcpClient tcpClient, int playerId)
        {
            TcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            PlayerId = playerId;
        }
    }
}