using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlickrNet;

namespace flickr.uploader.infrastructure
{
    public class FlickrService : IFlickrService
    {
        private readonly IConsole _console;
        private readonly IFileService _fileService;
        private Flickr _flickr;
        private readonly List<double> _lastSpeeds = new List<double> {0};
        private double _lastBytesSent = 0;
        private readonly Stopwatch _watch = new Stopwatch();

        // ----- Constructor
        public FlickrService(IConsole console, IFileService fileService)
        {
            _console = console;
            _fileService = fileService;
        }

        // ----- Public methods
        public void Authenticate(string apiKey, string apiSecret)
        {
            _flickr = GetAuthentifiedFlickrClient(apiKey, apiSecret);
            _flickr.OnUploadProgress += FlickrOnOnUploadProgress;
        }
        public Album GetAlbum(string albumId)
        {
            _console.Write($"* Loading album '{albumId}' ... ");
            try {
                CheckFlickrInitialized();
                var photoSet = _flickr.PhotosetsGetInfo(albumId);
                var allPhotosInAlbum = GetAllPhotosInAlbum(albumId, photoSet);
                _console.WriteLine("[OK]");
                return new Album {
                    Id = photoSet.PhotosetId,
                    Title = photoSet.Title,
                    Photos = allPhotosInAlbum.Select(x => new Photo {
                        Title = x.Title
                    })
                };
            }
            catch (Exception e) {
                _console.WriteLine($"[KO] => {e.Message}");
                throw;
            }
        }
        public void AddMediaFileInAlbum(MediaFile mediaFile, Album album)
        {
            _console.Write($"* Uploading '{mediaFile.FileName}' ... ");
            var photoId = UploadMediaFile(mediaFile);
            _console.Write(" Adding in the album ... ");
            _flickr.PhotosetsAddPhoto(album.Id, photoId);
            _console.WriteLine("[DONE]");
        }

        // ----- Callbacks
        private void FlickrOnOnUploadProgress(object sender, UploadProgressEventArgs args)
        {
            const int rowWidth = 40;
            if (args.UploadComplete) {
                var done = "[DONE]";
                _console.Write(done.PadRight(rowWidth));
                if (_console.CursorLeft - (rowWidth - done.Length) > 0) {
                    _console.SetCursorPosition(_console.CursorLeft - (rowWidth - done.Length), _console.CursorTop);
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

                var format = $"{args.ProcessPercentage} % ({ToBetterUnit(args.BytesSent)} on {ToBetterUnit(args.TotalBytesToSend)} - {ToBetterUnit((long)_lastSpeeds.Average(x => x))}/s)".PadRight(rowWidth);
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
        private Flickr GetAuthentifiedFlickrClient(string apiKey, string apiSecret)
        {
            const string tokenFilePath = ".flickr";
            if (!File.Exists(tokenFilePath)) {
                return CreateNewFlickrClient(apiKey, apiSecret, tokenFilePath);
            }
            try {
                return CreateFlickrClientFromExistingToken(apiKey, apiSecret, tokenFilePath);
            }
            catch (OAuthException) {
                return CreateNewFlickrClient(apiKey, apiSecret, tokenFilePath);
            }
        }
        private Flickr CreateFlickrClientFromExistingToken(string apiKey, string apiSecret, string tokenFilePath)
        {
            try {
                _console.Write($"* Reading token from '{tokenFilePath}' file ... ");
                var accesToken = _fileService.Deserialize<OAuthAccessToken>(tokenFilePath);
                var flickr = new Flickr(apiKey, apiSecret) {
                    OAuthAccessToken = accesToken.Token,
                    OAuthAccessTokenSecret = accesToken.TokenSecret
                };
                flickr.AuthOAuthCheckToken();
                _console.WriteLine("[OK]");
                return flickr;
            }
            catch (OAuthException e) {
                _console.WriteLine($"[KO] => {e.Message}");
                throw;
            }
        }
        private Flickr CreateNewFlickrClient(string apiKey, string apiSecret, string tokenFilePath)
        {
            var flickr = new Flickr(apiKey, apiSecret);
            var accesToken = CreateNewAccessToken(flickr);
            flickr.OAuthAccessToken = accesToken.Token;
            flickr.OAuthAccessTokenSecret = accesToken.TokenSecret;
            _fileService.Serialize(accesToken, tokenFilePath);
            _console.WriteLine($"* Access token saved in '{tokenFilePath}' file.");
            return flickr;
        }
        private OAuthAccessToken CreateNewAccessToken(Flickr flickr)
        {
            _console.WriteLine("* Creation of a new token.");
            var requestToken = flickr.OAuthGetRequestToken("oob");
            var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);
            _console.Write("* Opening browser ... ");
            Process.Start(url)?.WaitForExit();
            _console.WriteLine("[DONE]");

            while (true) {
                try {
                    _console.Write("* Type the Flickr verifier code : ");
                    var verifier = _console.ReadLine();
                    var accessToken = flickr.OAuthGetAccessToken(requestToken, verifier);
                    _console.WriteLine("* New access token acquired.");
                    return accessToken;
                }
                catch (OAuthException) {
                    _console.WriteLine("=> ERROR");
                }
            }
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