namespace flickr.uploader.domain.Removeduplication
{
    public class RemoveDuplicationInAlbumCommand : AuthenticatedCommand
    {
        public string AlbumId { get; set; }
    }
}