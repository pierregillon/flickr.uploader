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
    }
}