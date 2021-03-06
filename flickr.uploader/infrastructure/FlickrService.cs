﻿using System;
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
        private const string TEMPORARY_PHOTO_ID = "41751227641";

        private readonly IConsole _console;
        private Flickr _flickr;
        private readonly Stopwatch _watch = new Stopwatch();
        private OutputId _lastOuput;

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
        public string FindAlbumIdFromName(string albumName)
        {
            CheckFlickrInitialized();
            var photoSets = _flickr.PhotosetsGetList();
            var photoSet = photoSets.FirstOrDefault(x => x.Title == albumName);
            return photoSet?.PhotosetId;
        }
        public void AddMediaFileInAlbum(MediaFile mediaFile, Album album)
        {
            CheckFlickrInitialized();

            _watch.Restart();
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

            var newPhotoSet = _flickr.PhotosetsCreate(albumName, TEMPORARY_PHOTO_ID);
            return newPhotoSet.PhotosetId;
        }
        public void RemoveTemporaryPhoto(Album album)
        {
            var temporaryPhotoToCreateTheAlbum = album.Photos.FirstOrDefault(x => x.Id == TEMPORARY_PHOTO_ID);
            if (temporaryPhotoToCreateTheAlbum != null) {
                _console.StartOperation(
                    $"* Deleting temporary photo '{temporaryPhotoToCreateTheAlbum.Title}' ... ",
                    () => _flickr.PhotosetsRemovePhoto(album.Id, temporaryPhotoToCreateTheAlbum.Id));
            }
            else {
                _console.WriteLine("* No temporary photo found.");
            }
        }

        // ----- Callbacks
        private void FlickrOnOnUploadProgress(object sender, UploadProgressEventArgs args)
        {
            if (args.UploadComplete) {
                if (_lastOuput != null) {
                    _console.Clean(_lastOuput);
                }
                _watch.Stop();
            }
            else {
                var speed = args.BytesSent / _watch.ElapsedMilliseconds * 1000;
                var timeRemaining = speed == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds((args.TotalBytesToSend - args.BytesSent) / speed);
                var ouput = $"{args.ProcessPercentage} % ({args.BytesSent.ToOctets()} on {args.TotalBytesToSend.ToOctets()} - {speed.ToOctets()}/s, time remaning {timeRemaining})";
                if (_lastOuput != null) {
                    _console.Clean(_lastOuput);
                }
                _lastOuput = _console.Write(ouput);
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
    }
}