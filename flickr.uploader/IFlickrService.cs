namespace flickr.uploader
{
    public interface IFlickrService
    {
        Album GetAlbum(string albumId);

        void Authenticate(string apiKey, string apiSecret);
        void AddMediaFileInAlbum(MediaFile mediaFile, Album album);
    }
}