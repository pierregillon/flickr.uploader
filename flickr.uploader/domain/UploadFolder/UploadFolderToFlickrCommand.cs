namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommand : FlickrAuthenticatedCommand
    {
        public string LocalFolder { get; set; }
        public string AlbumId { get; set; }
    }
}