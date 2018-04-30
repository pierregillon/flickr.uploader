using System.Collections;
using System.Collections.Generic;

namespace flickr.uploader
{
    public interface IFileService
    {
        IReadOnlyCollection<MediaFile> GetMediaFiles(string folder);
        T Deserialize<T>(string filePath);
        void Serialize<T>(T obj, string filePath);
    }
}