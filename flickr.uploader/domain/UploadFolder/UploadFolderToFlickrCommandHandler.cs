using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using flickr.uploader.application;

namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommandHandler : ICommandHandler<UploadFolderToFlickrCommand>
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
            var album = LoadAlbum(command.AlbumId);
            var mediaFiles = ReadLocalFolder(command.LocalFolder);
            var mediaFilesFiltered = FilterOnlyNotAlreadyUploaded(mediaFiles, album);

            PrintStatement(command, album, mediaFiles, mediaFilesFiltered);
            ProcessFolder(mediaFilesFiltered, album, command.PromptUserConfirmation);

            _console.WriteLine("* Upload folder ended");
        }

        // ----- Internal logics
        private Album LoadAlbum(string albumId)
        {
            return _console.StartOperation(
                $"* Loading album '{albumId}' ... ",
                () => _flickrService.GetAlbum(albumId));
        }
        private IReadOnlyCollection<MediaFile> ReadLocalFolder(string localFolderPath)
        {
            return _console.StartOperation(
                $"* Reading folder '{localFolderPath}' ... ",
                () => _fileService.GetMediaFiles(localFolderPath));
        }
        private static IReadOnlyCollection<MediaFile> FilterOnlyNotAlreadyUploaded(IEnumerable<MediaFile> mediaFiles, Album album)
        {
            var existingTitles = album.Photos.Select(x => Normalize(x.Title)).ToArray();

            var query = from mediaFile in mediaFiles
                        let normalizedTitle = Normalize(mediaFile.FileName)
                        where existingTitles.Contains(normalizedTitle) == false
                        orderby mediaFile.Length
                        select mediaFile;
            return query.ToArray();
        }
        private void PrintStatement(UploadFolderToFlickrCommand command, Album album, IReadOnlyCollection<MediaFile> mediaFiles, IReadOnlyCollection<MediaFile> mediaFilesFiltered)
        {
            _console.WriteLine($"* Album is '{album.Title}' and contains {album.Photos.Count()} media files.");
            _console.WriteLine($"* Folder '{command.LocalFolder}' contains {mediaFiles.Count} media files with {mediaFiles.Count - mediaFilesFiltered.Count} already uploaded.");
        }
        private void ProcessFolder(IReadOnlyCollection<MediaFile> mediaFilesFiltered, Album album, bool promptUserConfirmation)
        {
            if (mediaFilesFiltered.Any()) {
                if (!promptUserConfirmation || ConfirmOperationByUser(mediaFilesFiltered)) {
                    AddMediaFilesInFlickrAlbum(mediaFilesFiltered, album);
                }
            }
            else {
                _console.WriteLine("* Nothing to upload.");
            }
        }
        private void AddMediaFilesInFlickrAlbum(IEnumerable<MediaFile> mediaFilesFiltered, Album album)
        {
            var data = mediaFilesFiltered.ToArray();
            for (var index = 0; index < data.Length; index++) {
                var mediaFile = data[index];
                try {
                    _console.StartOperation(
                        $"* [{DateTime.Now}] {index + 1}/{data.Length} Uploading '{mediaFile.FileName}' ... ",
                        () => _flickrService.AddMediaFileInAlbum(mediaFile, album));
                }
                catch (Exception) {
                    // continue to next file
                }
            }
        }
        private bool ConfirmOperationByUser(IReadOnlyCollection<MediaFile> mediaFilesFiltered)
        {
            return _console.ReadLine($"* {mediaFilesFiltered.Count} media files to upload. Continue? (y/n) => ") == "y";
        }
        private static string Normalize(string input)
        {
            return Path.GetFileNameWithoutExtension(input.ToLower());
        }
    }
}