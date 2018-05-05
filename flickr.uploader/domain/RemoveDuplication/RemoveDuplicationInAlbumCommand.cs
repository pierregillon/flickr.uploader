namespace flickr.uploader.domain.Removeduplication
{
    public class RemoveDuplicationInAlbumCommand : FlickrAuthenticatedCommand
    {
        public string AlbumId { get; set; }
    }
}