using System.Collections.Generic;

namespace flickr.uploader.domain
{
    public interface IFileService
    {
        IReadOnlyCollection<MediaFile> GetMediaFiles(string folder);
        T Deserialize<T>(string filePath);
        void Serialize<T>(T obj, string filePath);
        bool Exists(string filePath);
    }
}