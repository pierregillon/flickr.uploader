namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommand : AuthenticatedCommand
    {
        public string LocalFolder { get; set; }
        public string AlbumId { get; set; }
    }
}