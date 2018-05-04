using CommandLine;
using flickr.uploader.domain;
using flickr.uploader.domain.Removeduplication;
using flickr.uploader.domain.UploadFolder;
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

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(options => {
                      UploadFolder(container, options);
                      RemoveDuplication(container, options);
                  });
        }

        // ----- Utils
        private static void UploadFolder(IContainer container, Options options)
        {
            var handler = container.GetInstance<UploadFolderToFlickrCommandHandler>();
            var command = new UploadFolderToFlickrCommand {
                ApiKey = options.ApiKey,
                ApiSecret = options.ApiSecret,
                AlbumId = options.PhotoSetId,
                LocalFolder = options.LocalFolder
            };
            handler.Handle(command);
        }
        private static void RemoveDuplication(IContainer container, Options options)
        {
            var handler = container.GetInstance<RemoveDuplicationInAlbumCommandHandler>();
            var command = new RemoveDuplicationInAlbumCommand() {
                ApiKey = options.ApiKey,
                ApiSecret = options.ApiSecret,
                AlbumId = options.PhotoSetId,
            };
            handler.Handle(command);
        }
    }
}