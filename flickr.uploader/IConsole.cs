namespace flickr.uploader
{
    public interface IConsole
    {
        void Write(string input);
        void WriteLine(string input);
        string ReadLine();
        int CursorLeft { get; set; }
        int CursorTop { get; set; }
        void SetCursorPosition(int left, int top);
    }
}