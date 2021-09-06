using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulationTool.Helpers
{
    public class IoBox : IDisposable
    {
        public int port { get; set; }

        private Socket server { get; set; }

        private List<Socket> clients { get; set; }

        private CancellationTokenSource cancellationTokenSource { get; set; }

        public Subject<string> onMessage { get; } = new Subject<string>();

        #region Dispose
        private bool disposed { get; set; } = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Disconnect();
                }

                this.onMessage.OnCompleted();

                disposed = true;
            }
        }

        ~IoBox()
        {
            Dispose(false);
        }
        #endregion

        /// <summary>
        /// Connect
        /// </summary>
        public void Connect()
        {
            try
            {
                IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, this.port);

                this.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                this.server.Bind(iPEnd);

                this.server.Listen(100);

                this.clients = new List<Socket>();

                this.cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = this.cancellationTokenSource.Token;

                Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            Socket client = this.server.Accept();
                            EndPoint remote = client.RemoteEndPoint;

                            this.onMessage.OnNext($"Client<{remote}>: connect");

                            this.clients.Add(client);

                            Task.Run(() =>
                            {
                                try
                                {
                                    while (IsSocketConnected(client) && !token.IsCancellationRequested)
                                    {
                                        byte[] bytes = new byte[1024];

                                        int bytesRec = client.Receive(bytes);

                                        string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                                        message = message.Replace("\n", "").Replace("\r", "");

                                        if (!string.IsNullOrEmpty(message))
                                        {
                                            this.onMessage.OnNext($"Client<{remote}>: {message}");

                                            byte[] byteData = Encoding.ASCII.GetBytes($"OK\r\n");
                                            client.Send(byteData);
                                        }
                                    }

                                    client.Shutdown(SocketShutdown.Both);
                                    client.Close();
                                }
                                catch (Exception ex)
                                {
                                }
                                finally
                                {
                                    this.clients = this.clients.Where((n) => n.Handle != client.Handle).ToList();
                                    this.onMessage.OnNext($"Client<{remote}>: disconnect");
                                }
                            }, token);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }, token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                }

                if (this.clients != null)
                {
                    for (int i = 0; i < this.clients.Count(); i++)
                    {
                        Socket client = this.clients[i];

                        if (client.Connected)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }

                        client.Dispose();
                        client = null;
                    }
                }

                if (this.server != null)
                {
                    this.server.Close();
                    this.server.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// IsSocketConnected
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static bool IsSocketConnected(Socket client)
        {
            try
            {
                return !(client.Poll(1, SelectMode.SelectRead) && client.Available == 0);
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
