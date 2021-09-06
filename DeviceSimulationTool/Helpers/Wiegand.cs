using Min_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceSimulationTool.Helpers
{
    public class Wiegand : IDisposable
    {
        public int port { get; set; }

        private Socket server { get; set; }

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

        ~Wiegand()
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

                this.server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                this.server.Bind(iPEnd);

                this.cancellationTokenSource = new CancellationTokenSource();
                CancellationToken token = this.cancellationTokenSource.Token;

                Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        EndPoint remote = new IPEndPoint(IPAddress.Any, port);

                        byte[] bytes = new byte[1024];

                        try
                        {
                            int bytesRec = server.ReceiveFrom(bytes, ref remote);
                            if (bytesRec == 0) continue;

                            if (bytesRec == 14)
                            {
                                byte wiegandMode = bytes[10];

                                bytes = bytes.Skip(1).Take(9).ToArray();

                                string card = string.Join("", bytes.Select((n) => Convert.ToString(n, 2).PadLeft(8, '0')));
                                card = card.Substring(0, card.LastIndexOf("10101"));

                                if (wiegandMode == 35)
                                {
                                    string[] cards = Regex.Split(card, "").Where((n) => !string.IsNullOrEmpty(n)).Skip(card.Length == 36 ? 3 : 2).Reverse().Skip(1).ToArray();

                                    string head = string.Join("", cards.Skip(20).Reverse().ToArray());
                                    head = string.IsNullOrEmpty(head) ? "0" : head;
                                    head = Convert.ToInt64(head, 2).ToString().PadLeft(5, '0');

                                    string body = string.Join("", cards.Take(20).Reverse().ToArray());
                                    body = string.IsNullOrEmpty(body) ? "0" : body;
                                    body = Convert.ToInt64(body, 2).ToString().PadLeft(5, '0');

                                    this.onMessage.OnNext($"Client<{remote}>: {head,5}:{body,5} (HID iCLASS Corporate 1000 35-bit (遠傳使用))");
                                }
                                else if (wiegandMode == 34)
                                {
                                    string[] cards = Regex.Split(card, "").Where((n) => !string.IsNullOrEmpty(n)).Skip(card.Length == 35 ? 2 : 1).Reverse().Skip(1).ToArray();

                                    string head = string.Join("", cards.Skip(16).Reverse().ToArray());
                                    head = string.IsNullOrEmpty(head) ? "0" : head;
                                    head = Convert.ToInt64(head, 2).ToString().PadLeft(5, '0');

                                    string body = string.Join("", cards.Take(16).Reverse().ToArray());
                                    body = string.IsNullOrEmpty(body) ? "0" : body;
                                    body = Convert.ToInt64(body, 2).ToString().PadLeft(5, '0');

                                    this.onMessage.OnNext($"Client<{remote}>: {head,5}:{body,5} (標準Wiegand 34-bit (中大型企業用))");
                                }
                                else if (wiegandMode == 26)
                                {
                                    string[] cards = Regex.Split(card, "").Where((n) => !string.IsNullOrEmpty(n)).Skip(card.Length == 27 ? 2 : 1).Reverse().Skip(1).ToArray();

                                    string head = string.Join("", cards.Skip(16).Reverse().ToArray());
                                    head = string.IsNullOrEmpty(head) ? "0" : head;
                                    head = Convert.ToInt64(head, 2).ToString().PadLeft(5, '0');

                                    string body = string.Join("", cards.Take(16).Reverse().ToArray());
                                    body = string.IsNullOrEmpty(body) ? "0" : body;
                                    body = Convert.ToInt64(body, 2).ToString().PadLeft(5, '0');

                                    this.onMessage.OnNext($"Client<{remote}>: {head,5}:{body,5} (標準Wiegand 26-bit (遠傳使用))");
                                }
                            }
                            else if (bytesRec == 15)
                            {
                                byte type = bytes[13];
                                bytes = bytes.Skip(1).Take(10).ToArray();

                                string card = Encoding.ASCII.GetString(bytes);
                                card = card.PadLeft(10, '0');

                                if (type == 26)
                                {
                                    this.onMessage.OnNext($"Client<{remote}>: {card,10} (不分區Wiegand 26-bit (中小企業用))");
                                }
                                else if (type == 34)
                                {
                                    this.onMessage.OnNext($"Client<{remote}>: {card,10} (不分區Wiegand 34-bit (中小企業用))");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex = ExceptionHelper.GetReal(ex);
                            this.onMessage.OnNext($"{string.Join("", bytes.Select((n) => Convert.ToString(n, 16).PadLeft(2, '0'))).ToUpper()}, {ex.Message}");
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
    }
}
