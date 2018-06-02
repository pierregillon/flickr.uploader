using flickr.uploader.application;

namespace flickr.uploader.domain.CreateNewAlbum
{
    public class CreateNewAlbumCommandHandler : ICommandHandler<CreateNewAlbumCommand, string>
    {
        private readonly IConsole _console;
        private readonly IFlickrService _flickrService;

        public CreateNewAlbumCommandHandler(
            IConsole console,
            IFlickrService flickrService)
        {
            _console = console;
            _flickrService = flickrService;
        }

        public string Handle(CreateNewAlbumCommand command)
        {
            var albumName = _console.ReadLine("* Choose a name for the new album => ");
            return _console.StartOperation(
                $"* Creating album '{albumName}' ... ",
                () => _flickrService.CreateAlbum(albumName)
            );
        }
    }
}