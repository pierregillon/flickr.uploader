using FlickrNet;

namespace flickr.uploader.domain
{
    public interface IFlickrService
    {
        Album GetAlbum(string albumId);
        string FindAlbumIdFromName(string albumName);

        void Authenticate(Flickr flickr);
        void AddMediaFileInAlbum(MediaFile mediaFile, Album album);
        void DeletePhoto(Photo photo);
        string CreateAlbum(string albumName);
    }
}