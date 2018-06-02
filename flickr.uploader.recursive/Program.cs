using System.IO;
using CommandLine;
using flickr.uploader.application;
using flickr.uploader.domain;
using flickr.uploader.domain.Authenticate;
using flickr.uploader.domain.CreateNewAlbum;
using flickr.uploader.domain.RemoveDuplication;
using flickr.uploader.domain.UploadFolder;
using flickr.uploader.infrastructure;
using StructureMap;

namespace flickr.uploader.recursive
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
                      Process(dispatcher, options, container);
                  });
        }

        private static void Process(CommandDispatcher dispatcher, Options options, Container container)
        {
            var console = container.GetInstance<IConsole>();
            var flickrService = container.GetInstance<IFlickrService>();
            var directories = Directory.GetDirectories(options.LocalFolder, "[A UPLOADER]*");

            console.WriteLine($"* {directories.Length} directories found ready to be uploaded :");
            foreach (var directory in directories) {
                console.WriteLine($"\t - {directory}");
            }

            dispatcher.Dispatch(new AuthenticateCommand {
                ApiKey = options.ApiKey,
                ApiSecret = options.ApiSecret
            });

            foreach (var directoryPath in directories) {
                var albumName = Path.GetFileName(directoryPath)?.Replace("[A UPLOADER]", "").Trim();
                var albumId = flickrService.FindAlbumIdFromName(albumName);
                if (string.IsNullOrEmpty(albumId)) {
                    albumId = dispatcher.Dispatch<CreateNewAlbumCommand, string>(new CreateNewAlbumCommand {
                        AlbumName = albumName
                    });
                }

                if (string.IsNullOrEmpty(albumId) == false) {
                    dispatcher.Dispatch(new UploadFolderToFlickrCommand {
                        AlbumId = albumId,
                        LocalFolder = directoryPath,
                        PromptUserConfirmation = false
                    });
                    dispatcher.Dispatch(new RemoveDuplicationInAlbumCommand() {
                        AlbumId = albumId,
                        PromptUserConfirmation = false
                    });
                }
            }
        }
    }
}