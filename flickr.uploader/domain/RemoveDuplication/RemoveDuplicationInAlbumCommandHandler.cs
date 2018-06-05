using System.Linq;
using flickr.uploader.application;

namespace flickr.uploader.domain.RemoveDuplication
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

        // ----- Public methods
        public void Handle(RemoveDuplicationInAlbumCommand command)
        {
            var album = _flickrService.GetAlbum(command.AlbumId);

            RemoveTemporaryPhotoToCreateAlbum(album);
            RemoveDuplicatedPhotos(album, command.PromptUserConfirmation);

            _console.WriteLine("* Remove duplication ended");
        }

        // ----- Internal logics
        private void RemoveDuplicatedPhotos(Album album, bool promptUserConfirmation)
        {
            var duplicatedPhotoGroups = album.Photos.GroupBy(x => x.Title).Where(x => x.Count() > 1).ToArray();
            if (duplicatedPhotoGroups.Any()) {
                _console.WriteLine($"* {duplicatedPhotoGroups.Length} duplicated media files found in the album {album.Title}.");
                if (!promptUserConfirmation || ConfirmOperationByUser()) {
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
                _console.WriteLine($"* No duplication found in the album '{album.Title}'.");
            }
        }
        private bool ConfirmOperationByUser()
        {
            return _console.ReadLine("* Do you want to clean them up? (y/n) => ") == "y";
        }
        private void RemoveTemporaryPhotoToCreateAlbum(Album album)
        {
            var temporaryPhotoToCreateTheAlbum = album.Photos.FirstOrDefault(x => x.Id == "40995656465");
            if (temporaryPhotoToCreateTheAlbum != null) {
                _console.StartOperation(
                    $"* Deleting temporary photo '{temporaryPhotoToCreateTheAlbum.Title}' ... ",
                    () => _flickrService.DeletePhoto(temporaryPhotoToCreateTheAlbum));
            }
            else {
                _console.WriteLine("* No temporary photo found.");
            }
        }
    }
}