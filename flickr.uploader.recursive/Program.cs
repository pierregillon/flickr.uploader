using CommandLine;
using flickr.uploader.application;
using flickr.uploader.domain;
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
                      Process(dispatcher, options);
                  });
        }

        private static void Process(CommandDispatcher dispatcher, Options options)
        {
            throw new System.NotImplementedException();
        }
    }
}
