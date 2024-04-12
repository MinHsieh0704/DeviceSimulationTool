using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;

namespace DeviceSimulationTool.Helpers
{
    public class HttpCommand : IDisposable
    {
        #region Interface
        public class IAccount
        {
            public string account { get; set; }
            public string password { get; set; }
        }
        #endregion

        public int port { get; set; }

        public string account { get; set; }

        public string password { get; set; }

        private Process server { get; set; }

        public Subject<string> onMessage { get; } = new Subject<string>();

        public Subject<Exception> onError { get; } = new Subject<Exception>();

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
                this.onError.OnCompleted();

                disposed = true;
            }
        }

        ~HttpCommand()
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
                string file = $"{AppDomain.CurrentDomain.BaseDirectory}WebApiServer.exe";
                if (!File.Exists(file))
                {
                    throw new Exception($"Can not found {file}");
                }

                this.server = new Process();

                this.server.StartInfo.FileName = file;
                this.server.StartInfo.UseShellExecute = false;
                this.server.StartInfo.RedirectStandardOutput = true;
                this.server.StartInfo.RedirectStandardError = true;
                this.server.StartInfo.CreateNoWindow = true;
                this.server.StartInfo.Arguments = $"{App.AppProcessId} {this.port}{(string.IsNullOrEmpty(this.account) || string.IsNullOrEmpty(this.password) ? "" : $" {this.account} {this.password}")}";

                this.server.EnableRaisingEvents = true;

                this.server.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.onMessage.OnNext(e.Data.Replace("{{newline}}", "\r\n"));
                    }
                };
                this.server.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.onError.OnNext(new Exception(e.Data));
                    }
                };

                this.server.Start();

                this.server.BeginOutputReadLine();
                this.server.BeginErrorReadLine();
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
                if (this.server != null)
                {
                    this.server.CancelOutputRead();
                    this.server.CancelErrorRead();

                    if (!this.server.HasExited) this.server.Kill();
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
