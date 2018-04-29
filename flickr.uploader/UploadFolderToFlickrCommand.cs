using CommandLine;

namespace flickr.uploader
{
    public class UploadFolderToFlickrCommand
    {
        [Option("apikey", Required = true, HelpText = "The Flickr api key")]
        public string ApiKey { get; set; }
        [Option("apisecret", Required = true, HelpText = "The Flickr api secret")]
        public string ApiSecret { get; set; }
        public string FileName { get; set; }

        //[Option(Required = true, HelpText = "The Flick password")]
        //public string Password { get; set; }

        //[Option(Required = true, HelpText = "The folder to parse and to upload pictures")]
        //public string Folder { get; set; }

        //[Option(Required = true, HelpText = "The album name to upload the pictures.")]
        //public string Album { get; set; }
    }
}