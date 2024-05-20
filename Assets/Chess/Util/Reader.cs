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
        
        public static string ReadLine(int millisecondsTimeout = Timeout.Infinite) {
            _getInput.Set();
            return _gotInput.WaitOne(millisecondsTimeout) ? _input : null;
        }
    }
}