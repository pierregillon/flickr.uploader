using CommandLine;
using flickr.uploader.domain;
using flickr.uploader.domain.Authenticate;
using flickr.uploader.domain.CreateNewAlbum;
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
                x.For<IFlickrService>().Use<FlickrService>().Singleton();
            });

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(options => {
                      Authenticate(container, options);
                      if (string.IsNullOrEmpty(options.PhotoSetId)) {
                          options.PhotoSetId = CreateNewAlbum(container);
                      }
                      UploadFolder(container, options);
                      RemoveDuplication(container, options);
                  });
        }

        // ----- Internal logic
        private static void Authenticate(IContainer container, Options options)
        {
            var handler = container.GetInstance<AuthenticateCommandHandler>();
            var command = new AuthenticateCommand {
                ApiKey = options.ApiKey,
                ApiSecret = options.ApiSecret
            };
            handler.Handle(command);
        }
        private static string CreateNewAlbum(IContainer container)
        {
            var handler = container.GetInstance<CreateNewAlbumCommandHandler>();
            var command = new CreateNewAlbumCommand();
            return handler.Handle(command);
        }
        private static void UploadFolder(IContainer container, Options options)
        {
            var handler = container.GetInstance<UploadFolderToFlickrCommandHandler>();
            var command = new UploadFolderToFlickrCommand {
                AlbumId = options.PhotoSetId,
                LocalFolder = options.LocalFolder
            };
            handler.Handle(command);
        }
        private static void RemoveDuplication(IContainer container, Options options)
        {
            var handler = container.GetInstance<RemoveDuplicationInAlbumCommandHandler>();
            var command = new RemoveDuplicationInAlbumCommand() {
                AlbumId = options.PhotoSetId,
            };
            handler.Handle(command);
        }
    }
}