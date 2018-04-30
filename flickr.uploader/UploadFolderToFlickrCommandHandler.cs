using System;
using System.IO;
using System.Linq;
using System.Text;
using FlickrNet;
using Newtonsoft.Json;

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
            var flickr = GetAuthentifiedFlickrClient(command.ApiKey, command.ApiSecret);
            flickr.OnUploadProgress += (sender, args) => {
                if (args.UploadComplete) {
                    var done = "[DONE]";
                    Console.Write(done.PadRight(30));
                    if (Console.CursorLeft - (30 - done.Length) > 0) {
                        Console.SetCursorPosition(Console.CursorLeft - (30 - done.Length), Console.CursorTop);
                    }
                }
                else {
                    var format = $"{args.ProcessPercentage}% ({args.BytesSent} on {args.TotalBytesToSend})";
                    Console.Write(format);
                    Console.SetCursorPosition(Console.CursorLeft - format.Length, Console.CursorTop);
                }
            };
            Console.Write($"* Loading album '{command.PhotoSetId}' ... ");
            var album = flickr.PhotosetsGetPhotos(command.PhotoSetId);
            Console.WriteLine("[OK]");

            var allowedExtensions = new[] { ".jpg", ".mts", ".mp4"};

            var pictures = Directory
                .EnumerateFiles(command.PictureLocalFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                .ToArray();

            var missingPictures = (from pictureFile in pictures
                                   let pictureName = Path.GetFileName(pictureFile)
                                   where (from photo in album
                                          select photo.Title).Contains(pictureName) == false
                                   select new {
                                       Path = pictureFile,
                                       Name = pictureName
                                   }).ToArray();

            Console.WriteLine($"* Album is '{album.Title}' and contains {album.Count} photos.");
            Console.WriteLine($"* Folder '{command.PictureLocalFolder}' contains {pictures.Length} files with {pictures.Length - missingPictures.Length} already uploaded.");
            if (missingPictures.Any()) {
                Console.Write($"* {missingPictures.Length} files to upload. Continue? (y/n) => ");
                if (Console.ReadLine() == "y") {
                    foreach (var picture in missingPictures) {
                        Console.Write($"* Uploading '{picture.Name}'ttern ... ");
                        var photoId = flickr.UploadPicture(picture.Path, picture.Name, null, null, false, false, false);
                        Console.Write(" Adding in the album ... ");
                        flickr.PhotosetsAddPhoto(command.PhotoSetId, photoId);
                        Console.WriteLine("[DONE]");
                    }
                }
            }
            else {
                Console.WriteLine("* Nothing to do.");
            }

            Console.WriteLine("* Upload ended.");
            Console.ReadKey();
        }
        private static Flickr GetAuthentifiedFlickrClient(string apiKey, string apiSecret)
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
        private static Flickr CreateFlickrClientFromExistingToken(string apiKey, string apiSecret, string tokenFilePath)
        {
            try {
                Console.Write($"* Reading token from '{tokenFilePath}' file ... ");
                var accesToken = JsonConvert.DeserializeObject<OAuthAccessToken>(File.ReadAllText(tokenFilePath));
                var flickr = new Flickr(apiKey, apiSecret) {
                    OAuthAccessToken = accesToken.Token,
                    OAuthAccessTokenSecret = accesToken.TokenSecret
                };
                flickr.AuthOAuthCheckToken();
                Console.WriteLine("[OK]");
                return flickr;
            }
            catch (Exception e) {
                Console.WriteLine("[KO] => Invalid token");

                throw;
            }
        }
        private static Flickr CreateNewFlickrClient(string apiKey, string apiSecret, string tokenFilePath)
        {
            var flickr = new Flickr(apiKey, apiSecret);
            var accesToken = CreateNewAccessToken(flickr);
            flickr.OAuthAccessToken = accesToken.Token;
            flickr.OAuthAccessTokenSecret = accesToken.TokenSecret;
            File.WriteAllText(tokenFilePath, JsonConvert.SerializeObject(accesToken, Formatting.Indented));
            Console.WriteLine($"* Access token saved in '{tokenFilePath}' file.");
            return flickr;
        }
        private static OAuthAccessToken CreateNewAccessToken(Flickr flickr)
        {
            Console.WriteLine("* Creation of a new token.");
            var requestToken = flickr.OAuthGetRequestToken("oob");
            var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write);
            Console.Write("* Opening browser ... ");
            System.Diagnostics.Process.Start(url)?.WaitForExit();
            Console.WriteLine("[DONE]");

            while (true) {
                try {
                    Console.Write("* Type the Flickr verifier code : ");
                    var verifier = Console.ReadLine();
                    var accessToken = flickr.OAuthGetAccessToken(requestToken, verifier);
                    Console.WriteLine("* New access token acquired.");
                    return accessToken;
                }
                catch (OAuthException) {
                    Console.WriteLine("=> ERROR");
                }
            }
        }
    }

    public class FlickrToken
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}