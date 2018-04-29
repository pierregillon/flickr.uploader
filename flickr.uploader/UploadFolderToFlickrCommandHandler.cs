using System;
using FlickrNet;

namespace flickr.uploader
{
    public class UploadFolderToFlickrCommandHandler
    {
        public void Handle(UploadFolderToFlickrCommand command)
        {
            var flickr = new Flickr(command.ApiKey, command.ApiSecret);
            //f.OAuthAccessToken = OAuthToken.Token;
            //f.OAuthAccessTokenSecret = OAuthToken.TokenSecret;

            flickr.OnUploadProgress += (sender, args) => {
                Console.WriteLine(args.ProcessPercentage);
            };
            var requestToken = flickr.OAuthGetRequestToken("oob");
            var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);

            var p = System.Diagnostics.Process.Start(url);
            p.WaitForExit();

            string photoId = flickr.UploadPicture(command.FileName, "test", "test", null, false, false, false);
            
            //var options = new PhotoSearchOptions { Tags = "colorful", PerPage = 20, Page = 1 };
            //PhotoCollection photos = flickr.PhotosSearch(options);
            //foreach (Photo photo in photos)
            //{
            //    Console.WriteLine("Photo {0} has title {1}", photo.PhotoId, photo.Title);
            //}
            Console.ReadKey();
        }
    }
}