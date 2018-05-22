namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommand
    {
        public string LocalFolder { get; set; }
        public string AlbumName { get; set; }
    }
}