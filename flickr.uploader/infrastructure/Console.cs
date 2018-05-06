using System;
using flickr.uploader.domain;

namespace flickr.uploader.infrastructure
{
    public class Console : IConsole
    {
        public OutputId Write(string input)
        {
            var id = new OutputId {
                Left = CursorLeft,
                Length = input.Length
            };
            System.Console.Write(input);
            return id;
        }
        public void WriteLine(string input)
        {
            System.Console.WriteLine(input);
        }
        public string ReadLine()
        {
            return System.Console.ReadLine();
        }
        public string ReadLine(string instructions)
        {
            string result = null;
            while (string.IsNullOrEmpty(result)) {
                Write(instructions);
                result = ReadLine();
            }
            return result;
        }
        public void Clean(OutputId id)
        {
            SetCursorPosition(id.Left, CursorTop);
            Write("".PadRight(id.Length));
            SetCursorPosition(id.Left, CursorTop);
        }
        public int CursorLeft
        {
            get { return System.Console.CursorLeft; }
            set { System.Console.CursorLeft = value; }
        }
        public int CursorTop
        {
            get { return System.Console.CursorTop; }
            set { System.Console.CursorTop = value; }
        }
        public void SetCursorPosition(int left, int top)
        {
            System.Console.SetCursorPosition(left, top);
        }
        public T StartOperation<T>(string operationName, Func<T> action)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            if (action == null) throw new ArgumentNullException(nameof(action));

            try {
                Write(operationName);
                var result = action();
                WriteLine("[DONE]");
                return result;
            }
            catch (Exception ex) {
                WriteLine($"[FAIL] => {ex.Message}");
                throw;
            }
        }
        public void StartOperation(string operationName, Action action)
        {
            if (operationName == null) throw new ArgumentNullException(nameof(operationName));
            if (action == null) throw new ArgumentNullException(nameof(action));

            try {
                Write(operationName);
                action();
                WriteLine("[DONE]");
            }
            catch (Exception ex) {
                WriteLine($"[FAIL] => {ex.Message}");
                throw;
            }
        }
    }
}