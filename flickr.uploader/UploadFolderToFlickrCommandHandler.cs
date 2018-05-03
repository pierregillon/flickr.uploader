using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FlickrNet;
using Newtonsoft.Json;

namespace flickr.uploader
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
            var album = _flickrService.GetAlbum(command.PhotoSetId);
            var mediaFiles = _fileService.GetMediaFiles(command.PictureLocalFolder);
            var mediaFilesFiltered = FilterOnlyNotAlreadyUploaded(mediaFiles, album);
            _console.WriteLine($"* Album is '{album.Title}' and contains {album.Photos.Count()} photos.");
            _console.WriteLine($"* Folder '{command.PictureLocalFolder}' contains {mediaFiles.Count} media files with {mediaFiles.Count - mediaFilesFiltered.Count} already uploaded.");
            if (mediaFilesFiltered.Any()) {
                if (ConfirmOperationByUser(mediaFilesFiltered)) {
                    foreach (var mediaFile in mediaFilesFiltered) {
                        try {
                            _flickrService.AddMediaFileInAlbum(mediaFile, album);
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            else {
                _console.WriteLine("* Nothing to do.");
            }

            _console.WriteLine("* Upload folder to flickr operation ended.");
        }

        // ----- Internal logics
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
                        select mediaFile;

            return query.ToArray();
        }
    }
}