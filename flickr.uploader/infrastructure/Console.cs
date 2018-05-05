using System;
using flickr.uploader.domain;

namespace flickr.uploader.infrastructure
{
    public class Console : IConsole
    {
        public void Write(string input)
        {
            System.Console.Write(input);
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

            try
            {
                Write(operationName);
                var result = action();
                WriteLine("[DONE]");
                return result;
            }
            catch (Exception ex)
            {
                WriteLine($"[FAIL] => {ex.Message}");
                throw;
            }
        }
    }
}