using System.Linq;

namespace flickr.uploader.domain.RemoveDupplication
{
    public class RemoveDupplicationInAlbumCommandHandler
    {
        private readonly IConsole _console;
        private readonly IFlickrService _flickrService;

        public RemoveDupplicationInAlbumCommandHandler(
            IConsole console,
            IFlickrService flickrService)
        {
            _console = console;
            _flickrService = flickrService;
        }

        public void Handle(RemoveDupplicationInAlbumCommand command)
        {
            _flickrService.Authenticate(command.ApiKey, command.ApiSecret);
            RemoveDupplications(command.AlbumId);
        }

        private void RemoveDupplications(string albumId)
        {
            var album = _flickrService.GetAlbum(albumId);
            var dupplicatedPhotoGroups = album.Photos.GroupBy(x => x.Title).Where(x => x.Count() > 1).ToArray();
            if (dupplicatedPhotoGroups.Any()) {
                _console.WriteLine($"* {dupplicatedPhotoGroups.Length} dupplicated media files found in the album {album.Title}.");
                _console.Write("* Do you want to clean them up? (y/n) => ");
                if (_console.ReadLine() == "y") {
                    foreach (var dupplicatedPhotos in dupplicatedPhotoGroups) {
                        foreach (var photo in dupplicatedPhotos.Skip(1)) {
                            _flickrService.DeletePhoto(photo);
                        }
                    }
                    _console.WriteLine("* Album cleaned.");
                }
            }
            _console.WriteLine("* [END]");
        }
    }
}