using System;
using System.Collections.Generic;
using System.Linq;

namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommandHandler
    {
        private readonly IFlickrService _flickrService;
        private readonly IFileService _fileService;
        private readonly IConsole _console;

        // ----- Constructors
        public UploadFolderToFlickrCommandHandler(
            IFlickrService flickrService,
            IFileService fileService,
            IConsole console)
        {
            _flickrService = flickrService;
            _fileService = fileService;
            _console = console;
        }

        // ----- Public methods
        public void Handle(UploadFolderToFlickrCommand command)
        {
            _flickrService.Authenticate(command.ApiKey, command.ApiSecret);
            var album = _flickrService.GetAlbum(command.AlbumId);
            var mediaFiles = _fileService.GetMediaFiles(command.LocalFolder);
            var mediaFilesFiltered = FilterOnlyNotAlreadyUploaded(mediaFiles, album);
            PrintStatement(command, album, mediaFiles, mediaFilesFiltered);
            if (mediaFilesFiltered.Any()) {
                if (ConfirmOperationByUser(mediaFilesFiltered)) {
                    AddMediaFilesInFlickrAlbum(mediaFilesFiltered, album);
                }
            }
            else {
                _console.WriteLine("* Nothing to upload.");
            }
            _console.WriteLine("* [END]");
        }

        // ----- Internal logics
        private void AddMediaFilesInFlickrAlbum(IEnumerable<MediaFile> mediaFilesFiltered, Album album)
        {
            foreach (var mediaFile in mediaFilesFiltered) {
                try {
                    _flickrService.AddMediaFileInAlbum(mediaFile, album);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private void PrintStatement(UploadFolderToFlickrCommand command, Album album, IReadOnlyCollection<MediaFile> mediaFiles, IReadOnlyCollection<MediaFile> mediaFilesFiltered)
        {
            _console.WriteLine($"* Album is '{album.Title}' and contains {album.Photos.Count()} media files.");
            _console.WriteLine($"* Folder '{command.LocalFolder}' contains {mediaFiles.Count} media files with {mediaFiles.Count - mediaFilesFiltered.Count} already uploaded.");
        }
        private static bool ConfirmOperationByUser(IReadOnlyCollection<MediaFile> mediaFilesFiltered)
        {
            Console.Write($"* {mediaFilesFiltered.Count} media files to upload. Continue? (y/n) => ");
            return Console.ReadLine() == "y";
        }
        private static IReadOnlyCollection<MediaFile> FilterOnlyNotAlreadyUploaded(IEnumerable<MediaFile> mediaFiles, Album album)
        {
            var query = from mediaFile in mediaFiles
                        where (from photo in album.Photos
                               select photo.Title).Contains(mediaFile.FileName) == false
                        orderby mediaFile.Length
                        select mediaFile;

            return query.ToArray();
        }
    }
}