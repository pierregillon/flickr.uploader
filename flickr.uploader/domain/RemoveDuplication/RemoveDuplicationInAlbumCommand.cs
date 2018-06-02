namespace flickr.uploader.domain.RemoveDuplication
{
    public class RemoveDuplicationInAlbumCommand
    {
        public string AlbumId { get; set; }
        public bool PromptUserConfirmation { get; set; } = true;
    }
}