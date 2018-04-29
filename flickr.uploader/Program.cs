using System;
using CommandLine;

namespace flickr.uploader
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(RunOptionsAndReturnExitCode);
        }

        private static void RunOptionsAndReturnExitCode(Options options)
        {
            Console.WriteLine("done");
        }
    }

    class Options
    {
        [Option(Required = true, HelpText = "The Flickr login")]
        public string Login { get; set; }

        [Option(Required = true, HelpText = "The Flick password")]
        public string Password { get; set; }

        [Option(Required = true, HelpText = "The folder to parse and to upload pictures")]
        public string Folder { get; set; }

        [Option(Required = true, HelpText = "The album name to upload the pictures.")]
        public string Album { get; set; }
    }
}
