using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Network
{
    /// <summary>
    /// Handles network communication for both client and server operations
    /// </summary>
    public class NetworkManager : IDisposable
    {
        private readonly List<TcpClient> connectedClients;
        private bool disposed = false;

        public NetworkManager()
        {
            connectedClients = new List<TcpClient>();
        }

        /// <summary>
        /// Sends a message to a specific TCP client
        /// </summary>
        public async Task SendMessageToClientAsync(TcpClient client, string message)
        {
            if (client?.Connected != true || string.IsNullOrEmpty(message))
                return;

            try
            {
                byte[] messageBytes = EncodeMessage(message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error sending message to client: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients
        /// </summary>
        public async Task BroadcastMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            var clientsCopy = new List<TcpClient>(connectedClients);
            var tasks = new List<Task>();

            foreach (var client in clientsCopy)
            {
                if (client?.Connected == true)
                {
                    tasks.Add(SendMessageToClientAsync(client, message));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Reads a line of text from a TCP stream asynchronously
        /// </summary>
        public async Task<string> ReadLineAsync(System.IO.StreamReader reader)
        {
            try
            {
                return await reader.ReadLineAsync();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error reading from stream: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Adds a client to the managed connections list
        /// </summary>
        public void RegisterClient(TcpClient client)
        {
            if (client != null && !connectedClients.Contains(client))
            {
                connectedClients.Add(client);
            }
        }

        /// <summary>
        /// Removes a client from the managed connections list
        /// </summary>
        public void UnregisterClient(TcpClient client)
        {
            if (client != null)
            {
                connectedClients.Remove(client);
                SafeCloseClient(client);
            }
        }

        /// <summary>
        /// Gets the count of currently connected clients
        /// </summary>
        public int ConnectedClientCount => connectedClients.Count;

        /// <summary>
        /// Safely closes all client connections
        /// </summary>
        public void DisconnectAllClients()
        {
            var clientsCopy = new List<TcpClient>(connectedClients);
            connectedClients.Clear();

            foreach (var client in clientsCopy)
            {
                SafeCloseClient(client);
            }
        }

        private byte[] EncodeMessage(string message)
        {
            return Encoding.UTF8.GetBytes(message + "\n");
        }

        private void SafeCloseClient(TcpClient client)
        {
            try
            {
                client?.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error closing client connection: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                DisconnectAllClients();
                disposed = true;
            }
        }
    }
}