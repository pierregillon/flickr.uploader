using System.Linq;

namespace flickr.uploader.domain.Removeduplication
{
    public class RemoveDuplicationInAlbumCommandHandler
    {
        private readonly IConsole _console;
        private readonly IFlickrService _flickrService;

        public RemoveDuplicationInAlbumCommandHandler(
            IConsole console,
            IFlickrService flickrService)
        {
            _console = console;
            _flickrService = flickrService;
        }

        public void Handle(RemoveDuplicationInAlbumCommand command)
        {
            _flickrService.Authenticate(command.ApiKey, command.ApiSecret);
            RemoveDuplications(command.AlbumId);
        }

        private void RemoveDuplications(string albumId)
        {
            var album = _flickrService.GetAlbum(albumId);
            var duplicatedPhotoGroups = album.Photos.GroupBy(x => x.Title).Where(x => x.Count() > 1).ToArray();
            if (duplicatedPhotoGroups.Any()) {
                _console.WriteLine($"* {duplicatedPhotoGroups.Length} duplicated media files found in the album {album.Title}.");
                _console.Write("* Do you want to clean them up? (y/n) => ");
                if (_console.ReadLine() == "y") {
                    foreach (var duplicatedPhotos in duplicatedPhotoGroups) {
                        foreach (var photo in duplicatedPhotos.Skip(1)) {
                            _console.StartOperation(
                                $"* Deleting media file '{photo.Title}' ... ",
                                () => _flickrService.DeletePhoto(photo));
                        }
                    }
                    _console.WriteLine("* Album cleaned.");
                }
            }
            _console.WriteLine("* [END]");
        }
    }
}