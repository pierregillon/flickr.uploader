namespace flickr.uploader.domain.CreateNewAlbum {
    public class CreateNewAlbumCommandHandler
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
            _flickrService.Authenticate(command.ApiKey, command.ApiSecret);
            var albumName = _console.ReadLine("* Choose a name for the new album => ");
            return _console.StartOperation(
                $"* Creating album '{albumName}' ... ",
                () => _flickrService.CreateAlbum(albumName)
            );
        }
    }
}