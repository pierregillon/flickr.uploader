using CommandLine;
using flickr.uploader.domain;
using flickr.uploader.domain.Authenticate;
using flickr.uploader.domain.CreateNewAlbum;
using flickr.uploader.domain.Removeduplication;
using flickr.uploader.domain.UploadFolder;
using flickr.uploader.infrastructure;
using StructureMap;
using Console = flickr.uploader.infrastructure.Console;

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
                x.Scan(a => {
                    a.WithDefaultConventions();
                    a.AssemblyContainingType(typeof(ICommandHandler<>));
                    a.AddAllTypesOf(typeof(ICommandHandler<>));
                });
                x.Scan(a => {
                    a.WithDefaultConventions();
                    a.AssemblyContainingType(typeof(ICommandHandler<,>));
                    a.AddAllTypesOf(typeof(ICommandHandler<,>));
                });
            });

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(options => {
                      var dispatcher = container.GetInstance<CommandDispatcher>();
                      Process(dispatcher, options);
                  });
        }

        private static void Process(CommandDispatcher dispatcher, Options options)
        {
            dispatcher.Dispatch(new AuthenticateCommand {
                ApiKey = options.ApiKey,
                ApiSecret = options.ApiSecret
            });
            //if (string.IsNullOrEmpty(options.PhotoSetId)) {
            //    var photosetId = dispatcher.Dispatch<CreateNewAlbumCommand, string>(new CreateNewAlbumCommand());
            //    options.PhotoSetId = photosetId;
            //}
            dispatcher.Dispatch(new UploadFolderToFlickrCommand {
                AlbumName = options.PhotoSetName,
                LocalFolder = options.LocalFolder
            });
            dispatcher.Dispatch(new RemoveDuplicationInAlbumCommand() {
                AlbumId = options.PhotoSetName,
            });
        }
    }
}