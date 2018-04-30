using CommandLine;
using flickr.uploader.infrastructure;
using StructureMap;

namespace flickr.uploader
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container(x => {
                x.For<IConsole>().Use<Console>();
                x.For<IFileService>().Use<FileService>();
                x.For<IFlickrService>().Use<FlickrService>();
            });
            var handler = container.GetInstance<UploadFolderToFlickrCommandHandler>();
            Parser.Default.ParseArguments<UploadFolderToFlickrCommand>(args)
                  .WithParsed(handler.Handle);
        }
    }
}