using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using flickr.uploader.domain;
using FlickrNet;
using Photo = flickr.uploader.domain.Photo;

namespace flickr.uploader.infrastructure
{
    public class FlickrService : IFlickrService
    {
        private readonly IConsole _console;
        private Flickr _flickr;
        private readonly List<double> _lastSpeeds = new List<double> { 0 };
        private double _lastBytesSent = 0;
        private readonly Stopwatch _watch = new Stopwatch();

        // ----- Constructor
        public FlickrService(IConsole console)
        {
            _console = console;
        }

        // ----- Public methods
        public void Authenticate(Flickr flickr)
        {
            _flickr = flickr;
            _flickr.OnUploadProgress += FlickrOnOnUploadProgress;
        }
        public Album GetAlbum(string albumId)
        {
            CheckFlickrInitialized();
            var photoSet = _flickr.PhotosetsGetInfo(albumId);
            var allPhotosInAlbum = GetAllPhotosInAlbum(albumId, photoSet);
            return new Album {
                Id = photoSet.PhotosetId,
                Title = photoSet.Title,
                Photos = allPhotosInAlbum.Select(x => new Photo {
                    Id = x.PhotoId,
                    Title = x.Title
                })
            };
        }
        public void AddMediaFileInAlbum(MediaFile mediaFile, Album album)
        {
            CheckFlickrInitialized();

            var photoId = UploadMediaFile(mediaFile);
            var position = _console.Write(" Adding in the album ... ");
            _flickr.PhotosetsAddPhoto(album.Id, photoId);
            _console.Clean(position);
        }
        public void DeletePhoto(Photo photo)
        {
            CheckFlickrInitialized();

            _flickr.PhotosDelete(photo.Id);
        }
        public string CreateAlbum(string albumName)
        {
            CheckFlickrInitialized();

            var photoSet = _flickr.PhotosetsCreate(albumName, "P1040475.JPG");
            _flickr.PhotosetsRemovePhoto(photoSet.PhotosetId, "P1040475.JPG");
            return photoSet.PhotosetId;
        }

        // ----- Callbacks
        private void FlickrOnOnUploadProgress(object sender, UploadProgressEventArgs args)
        {
            const int rowWidth = 40;
            if (args.UploadComplete) {
                var clean = " ";
                _console.Write(clean.PadRight(rowWidth));
                if (_console.CursorLeft - (rowWidth - clean.Length) > 0) {
                    _console.SetCursorPosition(_console.CursorLeft - (rowWidth - clean.Length), _console.CursorTop);
                }
            }
            else {
                if (_lastBytesSent != 0) {
                    var bytesSentInOperation = args.BytesSent - _lastBytesSent;
                    var ellapsedSeconds = _watch.Elapsed.TotalSeconds;
                    var bytesPerSeconds = bytesSentInOperation / ellapsedSeconds;
                    _lastSpeeds.Add(bytesPerSeconds);
                    if (_lastSpeeds.Count > 30) {
                        _lastSpeeds.RemoveAt(0);
                    }
                }
                _lastBytesSent = args.BytesSent;
                _watch.Restart();

                var format = $"{args.ProcessPercentage} % ({ToBetterUnit(args.BytesSent)} on {ToBetterUnit(args.TotalBytesToSend)} - {ToBetterUnit((long) _lastSpeeds.Average(x => x))}/s)".PadRight(rowWidth);
                _console.Write(format);
                _console.SetCursorPosition(_console.CursorLeft - format.Length, _console.CursorTop);
            }
        }

        // ----- Internal logic
        private void CheckFlickrInitialized()
        {
            if (_flickr == null) {
                throw new Exception("Flickr not initialized");
            }
        }
        private IEnumerable<FlickrNet.Photo> GetAllPhotosInAlbum(string albumId, Photoset photoSet)
        {
            var allPhotosInAlbum = new List<FlickrNet.Photo>();
            const int elementPerPageCount = 500;
            var pageCount = (int) Math.Ceiling((float) (photoSet.NumberOfPhotos + photoSet.NumberOfVideos) / elementPerPageCount);
            for (var pageNumber = 1; pageNumber <= pageCount; pageNumber++) {
                var photosPerPage = _flickr.PhotosetsGetPhotos(albumId, PhotoSearchExtras.None, PrivacyFilter.None, pageNumber, elementPerPageCount);
                allPhotosInAlbum.AddRange(photosPerPage);
            }
            return allPhotosInAlbum;
        }
        private string UploadMediaFile(MediaFile mediaFile)
        {
            string photoId;
            using (var stream = File.OpenRead(mediaFile.Path)) {
                photoId = _flickr.UploadPicture(
                    stream,
                    mediaFile.Path,
                    mediaFile.FileName,
                    null,
                    null,
                    false,
                    false,
                    false,
                    ContentType.Photo,
                    SafetyLevel.None,
                    HiddenFromSearch.Hidden);
            }
            return photoId;
        }
        

        // ----- Utils
        private static string ToBetterUnit(long bytes)
        {
            var _1_KO = Math.Pow(2, 10);
            var _1_MO = Math.Pow(2, 20);
            var _1_GO = Math.Pow(2, 30);

            if (bytes >= _1_GO) {
                return $"{Math.Round(bytes / _1_GO)} Go";
            }
            if (bytes >= _1_MO) {
                return $"{Math.Round(bytes / _1_MO)} Mo";
            }
            if (bytes >= _1_KO) {
                return $"{Math.Round(bytes / _1_KO)} Ko";
            }
            return $"{bytes} o";
        }
    }
}