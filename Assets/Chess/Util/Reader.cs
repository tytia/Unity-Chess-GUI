using System;
using System.Threading;

namespace Chess.Util {
    public static class Reader {
        private static readonly AutoResetEvent _getInput, _gotInput;
        private static string _input;

        static Reader() {
            _getInput = new AutoResetEvent(false);
            _gotInput = new AutoResetEvent(false);
            var inputThread = new Thread(Read) {
                IsBackground = true
            };
            inputThread.Start();
        }

        private static void Read() {
            while (true) {
                _getInput.WaitOne();
                _input = Console.ReadLine();
                _gotInput.Set();
            }
        }
        
        public static bool TryReadLine(out string line, int timeout = Timeout.Infinite) {
            _getInput.Set();
            bool success = _gotInput.WaitOne(timeout);
            
            line = success ? _input : null;
            return success;
        }
    }
}