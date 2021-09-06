using Min_Helpers;
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
    public class AO20WWG : IDisposable
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

        ~AO20WWG()
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

                                        try
                                        {
                                            int bytesRec = client.Receive(bytes);
                                            if (bytesRec == 0) continue;

                                            if (bytes[0] == 49)
                                            {
                                                byte wiegandMode = bytes[2];

                                                bytes = bytes.Skip(3).Take(bytesRec - 3).ToArray();

                                                if (wiegandMode == 24)
                                                {
                                                    string card = string.Join("", bytes.Select((n) => Convert.ToString(n, 2).PadLeft(8, '0')));

                                                    string head = string.Join("", card.Substring(0, 8));
                                                    head = string.IsNullOrEmpty(head) ? "0" : head;
                                                    head = Convert.ToInt64(head, 2).ToString().PadLeft(5, '0');

                                                    string body = string.Join("", card.Substring(8, 16));
                                                    body = string.IsNullOrEmpty(body) ? "0" : body;
                                                    body = Convert.ToInt64(body, 2).ToString().PadLeft(5, '0');

                                                    card = Convert.ToInt64(card, 2).ToString().PadLeft(10, '0');

                                                    this.onMessage.OnNext($"Client<{remote}>: {head,5}:{body,5} (iClass Wiegand 26-bit)\r\n{new string(' ', remote.ToString().Length + 35)}{card,10} (Mifare Wiegand 26-bit)");
                                                }
                                                else if (wiegandMode == 32)
                                                {
                                                    string card = string.Join("", bytes.Select((n) => Convert.ToString(n, 2).PadLeft(8, '0')));

                                                    string head = string.Join("", card.Substring(0, 16));
                                                    head = string.IsNullOrEmpty(head) ? "0" : head;
                                                    head = Convert.ToInt64(head, 2).ToString().PadLeft(5, '0');

                                                    string body = string.Join("", card.Substring(16, 16));
                                                    body = string.IsNullOrEmpty(body) ? "0" : body;
                                                    body = Convert.ToInt64(body, 2).ToString().PadLeft(5, '0');

                                                    card = Convert.ToInt64(card, 2).ToString().PadLeft(10, '0');

                                                    this.onMessage.OnNext($"Client<{remote}>: {head,5}:{body,5} (iClass Wiegand 34-bit)\r\n{new string(' ', remote.ToString().Length + 35)}{card,10} (Mifare Wiegand 34-bit)");
                                                }
                                            }

                                            client.Send(new byte[] { 0x40 });
                                        }
                                        catch (Exception ex)
                                        {
                                            ex = ExceptionHelper.GetReal(ex);
                                            this.onMessage.OnNext($"{string.Join("", bytes.Select((n) => Convert.ToString(n, 16).PadLeft(2, '0'))).ToUpper()}, {ex.Message}");

                                            client.Send(new byte[] { 0x40 });
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
