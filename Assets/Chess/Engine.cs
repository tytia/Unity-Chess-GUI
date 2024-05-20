using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Timers;
using Chess.Util;

namespace Chess {
    /// <summary>
    /// Represents a UCI-compatible chess engine.
    /// </summary>
    public class Engine {
        public string path { get; set; }
        public bool ready { get; private set; }
        private AnonymousPipeServerStream _server;
        private Process _engine;
        private StreamWriter _sw;

        public event EventHandler UCIResponseTimeout;

        public Engine(string path) {
            this.path = path;
        }

        public void StartProcess() {
            _server = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _engine = new Process();
            _sw = new StreamWriter(_server) {AutoFlush = true};
            
            _engine.StartInfo.FileName = path;
            _engine.StartInfo.Arguments = _server.GetClientHandleAsString();
            _engine.StartInfo.UseShellExecute = false;
            _engine.Start();
            
            _server.DisposeLocalCopyOfClientHandle();

            try {
                _sw.WriteLine("uci");
                ready = Reader.ReadLine(5000) == "uciok";
                if (!ready) {
                    UCIResponseTimeout?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (IOException e) {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        public void KillProcess() {
            _engine?.Kill();
            _engine?.Dispose();
            
            _server?.Dispose();
            _sw?.Dispose();
        }
    }
}