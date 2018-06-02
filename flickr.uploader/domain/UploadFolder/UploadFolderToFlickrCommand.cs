namespace flickr.uploader.domain.UploadFolder
{
    public class UploadFolderToFlickrCommand
    {
        public string LocalFolder { get; set; }
        public string AlbumId { get; set; }
        public bool PromptUserConfirmation { get; set; } = true;
    }
}