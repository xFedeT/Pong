using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pong.Core;
using Pong.Client.Graphics;
using Pong.Network;
using Timer = System.Windows.Forms.Timer;

namespace Pong.Client
{
    /// <summary>
    /// Main client form that handles user interface and server communication
    /// </summary>
    public partial class GameClient : Form
    {
        private readonly NetworkManager networkManager;
        private readonly GameRenderer gameRenderer;
        private readonly GameState localGameState;
        
        private TcpClient serverConnection;
        private int assignedPlayerId;
        private bool isGameActive = false;
        
        // UI Components
        private Label scoreDisplayLabel;
        private Label countdownDisplayLabel;
        private Panel gameRenderPanel;
        private Timer uiUpdateTimer;

        public GameClient()
        {
            networkManager = new NetworkManager();
            gameRenderer = new GameRenderer();
            localGameState = new GameState();
            
            InitializeUserInterface();
            SetupEventHandlers();
        }

        private void InitializeUserInterface()
        {
            ConfigureMainWindow();
            CreateUIComponents();
            SetupRenderingTimer();
        }

        private void ConfigureMainWindow()
        {
            Text = "Pong Multiplayer - Client";
            Width = GameConfiguration.FieldWidth + GameConfiguration.WindowExtraWidth;
            Height = GameConfiguration.FieldHeight + GameConfiguration.WindowExtraHeight;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = System.Drawing.Color.Black;
        }

        private void CreateUIComponents()
        {
            scoreDisplayLabel = gameRenderer.CreateScoreLabel();
            countdownDisplayLabel = gameRenderer.CreateCountdownLabel();
            gameRenderPanel = gameRenderer.CreateGamePanel();

            Controls.Add(gameRenderPanel);
            Controls.Add(countdownDisplayLabel);
            Controls.Add(scoreDisplayLabel);
        }

        private void SetupRenderingTimer()
        {
            uiUpdateTimer = new Timer 
            { 
                Interval = GameConfiguration.GameTimerInterval 
            };
            uiUpdateTimer.Tick += OnRenderTimerTick;
            uiUpdateTimer.Start();
        }

        private void SetupEventHandlers()
        {
            Shown += OnFormShown;
            KeyDown += OnKeyPressed;
            gameRenderPanel.Paint += OnGamePanelPaint;
            FormClosing += OnFormClosing;
        }

        private async void OnFormShown(object sender, EventArgs e)
        {
            await InitiateServerConnectionAsync();
        }

        private void OnRenderTimerTick(object sender, EventArgs e)
        {
            gameRenderPanel?.Invalidate();
        }

        private void OnGamePanelPaint(object sender, PaintEventArgs e)
        {
            gameRenderer.RenderGame(e, localGameState);
        }

        private async void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (!isGameActive || serverConnection?.Connected != true)
                return;

            int movementDirection = DetermineMovementDirection(e.KeyCode);
            if (movementDirection != 0)
            {
                await SendMovementCommandAsync(movementDirection);
            }
        }

        private int DetermineMovementDirection(Keys keyCode)
        {
            return keyCode switch
            {
                Keys.Up => -1,
                Keys.Down => 1,
                _ => 0
            };
        }

        private async Task SendMovementCommandAsync(int direction)
        {
            try
            {
                string command = $"MOVE:{direction}";
                await networkManager.SendMessageToClientAsync(serverConnection, command);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to send movement command: {ex.Message}");
            }
        }

        private async Task InitiateServerConnectionAsync()
        {
            while (true)
            {
                if (!await EstablishServerConnectionAsync())
                    return;

                await HandleServerCommunicationAsync();

                // Connection lost, show reconnection dialog
                var reconnectResult = MessageBox.Show(
                    "Connection lost. The other player may have disconnected.\nWould you like to try connecting again?",
                    "Connection Lost",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (reconnectResult == DialogResult.No)
                    return;

                ResetClientState();
            }
        }

        private async Task<bool> EstablishServerConnectionAsync()
        {
            string serverAddress = PromptForServerAddress();
            if (string.IsNullOrWhiteSpace(serverAddress))
                return false;

            try
            {
                serverConnection = new TcpClient();
                await serverConnection.ConnectAsync(serverAddress, GameConfiguration.ServerPort);
                networkManager.RegisterClient(serverConnection);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}", 
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string PromptForServerAddress()
        {
            return Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the server IP address:", 
                "Connect to Server", 
                GameConfiguration.DefaultServerIP);
        }

        private async Task HandleServerCommunicationAsync()
        {
            try
            {
                using var streamReader = new System.IO.StreamReader(
                    serverConnection.GetStream(), Encoding.UTF8);

                while (serverConnection.Connected)
                {
                    string serverMessage = await networkManager.ReadLineAsync(streamReader);
                    if (serverMessage == null)
                        break;

                    await ProcessServerMessageAsync(serverMessage);

                    if (serverMessage == "QUIT")
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Communication error: {ex.Message}");
            }
            finally
            {
                CleanupConnection();
            }
        }

        private async Task ProcessServerMessageAsync(string message)
        {
            var messageType = ExtractMessageType(message);
            var messageContent = ExtractMessageContent(message);

            switch (messageType)
            {
                case "ASSIGN":
                    HandlePlayerAssignment(messageContent);
                    break;

                case "COUNTDOWN":
                    HandleCountdownUpdate(messageContent);
                    break;

                case "START":
                    HandleGameStart();
                    break;

                case "STATE":
                    HandleGameStateUpdate(messageContent);
                    break;

                case "QUIT":
                    HandleServerQuit();
                    break;
            }
        }

        private string ExtractMessageType(string message)
        {
            int colonIndex = message.IndexOf(':');
            return colonIndex > 0 ? message.Substring(0, colonIndex) : message;
        }

        private string ExtractMessageContent(string message)
        {
            int colonIndex = message.IndexOf(':');
            return colonIndex > 0 ? message.Substring(colonIndex + 1) : "";
        }

        private void HandlePlayerAssignment(string playerIdString)
        {
            if (int.TryParse(playerIdString, out int playerId))
            {
                assignedPlayerId = playerId;
                System.Console.WriteLine($"Assigned as Player {assignedPlayerId}");
            }
        }

        private void HandleCountdownUpdate(string countdownText)
        {
            gameRenderer.UpdateCountdownDisplay(countdownDisplayLabel, countdownText);
        }

        private void HandleGameStart()
        {
            gameRenderer.UpdateCountdownDisplay(countdownDisplayLabel, "");
            isGameActive = true;
        }

        private void HandleGameStateUpdate(string stateData)
        {
            ParseAndUpdateGameState(stateData);
            gameRenderer.UpdateScoreDisplay(scoreDisplayLabel, 
                localGameState.Player1Score, localGameState.Player2Score);
        }

        private void HandleServerQuit()
        {
            isGameActive = false;
            ShowVictoryMessage();
        }

        private void ParseAndUpdateGameState(string stateData)
        {
            var stateComponents = stateData.Split(',');
            if (stateComponents.Length != 6)
                return;

            try
            {
                localGameState.BallPositionX = int.Parse(stateComponents[0]);
                localGameState.BallPositionY = int.Parse(stateComponents[1]);
                localGameState.Player1PaddleY = int.Parse(stateComponents[2]);
                localGameState.Player2PaddleY = int.Parse(stateComponents[3]);
                localGameState.Player1Score = int.Parse(stateComponents[4]);
                localGameState.Player2Score = int.Parse(stateComponents[5]);
            }
            catch (FormatException ex)
            {
                System.Console.WriteLine($"Error parsing game state: {ex.Message}");
            }
        }

        private void ShowVictoryMessage()
        {
            Invoke(new Action(() =>
            {
                MessageBox.Show("The other player disconnected.\nYou win!",
                    "Victory", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        private void ResetClientState()
        {
            isGameActive = false;
            localGameState.Reset();
            gameRenderer.UpdateCountdownDisplay(countdownDisplayLabel, "");
            gameRenderer.UpdateScoreDisplay(scoreDisplayLabel, 0, 0);
        }

        private void CleanupConnection()
        {
            if (serverConnection != null)
            {
                networkManager.UnregisterClient(serverConnection);
                serverConnection = null;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            CleanupConnection();
            uiUpdateTimer?.Stop();
            networkManager?.Dispose();
        }
    }
}