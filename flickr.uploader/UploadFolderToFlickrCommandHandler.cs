using System;
using System.IO;
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
            var flickr = new Flickr(command.ApiKey, command.ApiSecret) {
                OAuthAccessToken = "72157695536540544-7216367b90233f5d",
                OAuthAccessTokenSecret = "836a4b12efc6244e"
            };

            try {
                flickr.AuthOAuthCheckToken();
            }
            catch (Exception e) {
                var requestToken = flickr.OAuthGetRequestToken("oob");
                var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);
                var p = System.Diagnostics.Process.Start(url);
                p.WaitForExit();

                var accessToken = flickr.OAuthGetAccessToken(requestToken, "190-368-949");
                flickr.OAuthAccessToken = accessToken.Token;
                flickr.OAuthAccessTokenSecret = accessToken.TokenSecret;
                throw;
            }
           


            var pictures = Directory.GetFiles(command.PictureLocalFolder, "*.jpg", SearchOption.AllDirectories);
            foreach (var picture in pictures) {

                //var photoSets = flickr.PhotosetsGetList();

              
                flickr.OnUploadProgress += (sender, args) => {
                    Console.WriteLine(args.ProcessPercentage);
                };
                //string photoId = flickr.UploadPicture(picture, Path.GetFileName(picture), null, null, false, false, false);

                //var options = new PhotoSearchOptions { Tags = "colorful", PerPage = 20, Page = 1 };
                //PhotoCollection photos = flickr.PhotosSearch(options);
                //foreach (Photo photo in photos)
                //{
                //    Console.WriteLine("Photo {0} has title {1}", photo.PhotoId, photo.Title);
                //}
            }
            Console.ReadKey();
        }
    }

    public class FlickrToken
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}