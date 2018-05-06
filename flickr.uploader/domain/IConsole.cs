using System;

namespace flickr.uploader.domain
{
    public interface IConsole
    {
        OutputId Write(string input);
        void WriteLine(string input);
        string ReadLine();
        string ReadLine(string instructions);
        void Clean(OutputId id);
        int CursorLeft { get; set; }
        int CursorTop { get; set; }
        void SetCursorPosition(int left, int top);
        T StartOperation<T>(string operationName, Func<T> action);
        void StartOperation(string operationName, Action action);
    }
}