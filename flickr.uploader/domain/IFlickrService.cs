using FlickrNet;

namespace flickr.uploader.domain
{
    public interface IFlickrService
    {
        Album GetAlbum(string albumId);

        void Authenticate(Flickr flickr);
        void AddMediaFileInAlbum(MediaFile mediaFile, Album album);
        void DeletePhoto(Photo photo);
        string CreateAlbum(string albumName);
    }
}