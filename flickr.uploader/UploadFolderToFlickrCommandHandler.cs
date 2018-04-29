using System;
using System.IO;
using System.Linq;
using FlickrNet;

namespace flickr.uploader
{
    public class UploadFolderToFlickrCommandHandler
    {
        private readonly FlickrToken _token;

        public UploadFolderToFlickrCommandHandler(FlickrToken token)
        {
            _token = token;
        }

        public void Handle(UploadFolderToFlickrCommand command)
        {
            var flickr = GetAuthentifiedFlickrClient(command);
            flickr.OnUploadProgress += (sender, args) => {
                if (args.UploadComplete) {
                    Console.Write("[DONE]");
                }
            };
            var album = flickr.PhotosetsGetPhotos(command.PhotoSetId);
            var pictures = Directory.GetFiles(command.PictureLocalFolder, "*.jpg", SearchOption.AllDirectories);
            foreach (var picture in pictures) {
                var photoTitle = Path.GetFileName(picture);
                if (album.Any(x => x.Title == photoTitle) == false) {
                    Console.Write("* Photo '' missing. Uploading ... ");
                    var photoId = flickr.UploadPicture(picture, photoTitle, null, null, false, false, false);
                    Console.Write("Adding in album ... ");
                    flickr.PhotosetsAddPhoto(command.PhotoSetId, photoId);
                    Console.WriteLine("[DONE]");
                }
                else {
                    Console.WriteLine($"* Photo '{photoTitle}' already exists in the album '{album.Title}'.");
                }
            }
            Console.ReadKey();
        }
        private static Flickr GetAuthentifiedFlickrClient(UploadFolderToFlickrCommand command)
        {
            var flickr = new Flickr(command.ApiKey, command.ApiSecret) {
                OAuthAccessToken = "72157695536540544-7216367b90233f5d",
                OAuthAccessTokenSecret = "836a4b12efc6244e"
            };

            try {
                flickr.AuthOAuthCheckToken();
                return flickr;
            }
            catch (Exception) {
                var requestToken = flickr.OAuthGetRequestToken("oob");
                var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);
                System.Diagnostics.Process.Start(url)?.WaitForExit();
                var verifier = Console.ReadLine();
                var accessToken = flickr.OAuthGetAccessToken(requestToken, verifier);
                flickr.OAuthAccessToken = accessToken.Token;
                flickr.OAuthAccessTokenSecret = accessToken.TokenSecret;
                return flickr;
            }
        }
    }

    public class FlickrToken
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}