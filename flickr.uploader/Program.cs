using System;
using CommandLine;
using StructureMap;

namespace flickr.uploader
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
            var handler = container.GetInstance<UploadFolderToFlickrCommandHandler>();
            Parser.Default.ParseArguments<UploadFolderToFlickrCommand>(args)
                  .WithParsed(handler.Handle);
        }
    }
}
