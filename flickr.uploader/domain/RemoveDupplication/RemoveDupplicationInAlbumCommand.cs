namespace flickr.uploader.domain.RemoveDupplication
{
    public class RemoveDupplicationInAlbumCommand : AuthenticatedCommand
    {
        public string AlbumId { get; set; }
    }
}