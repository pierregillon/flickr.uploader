using System.Linq;

namespace flickr.uploader.domain.Removeduplication
{
    public class RemoveDuplicationInAlbumCommandHandler : ICommandHandler<RemoveDuplicationInAlbumCommand>
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
            var album = _flickrService.GetAlbum(command.AlbumId);
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
            else {
                _console.WriteLine($"* No duplication found on the album '{album.Title}'.");
            }
            _console.WriteLine("* Remove duplication ended");
        }
    }
}