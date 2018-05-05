using System;

namespace flickr.uploader.domain
{
    public interface IConsole
    {
        void Write(string input);
        void WriteLine(string input);
        string ReadLine();
        string ReadLine(string instructions);
        int CursorLeft { get; set; }
        int CursorTop { get; set; }
        void SetCursorPosition(int left, int top);
        T StartOperation<T>(string operationName, Func<T> action);
    }
}