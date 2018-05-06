using System.Collections.Generic;
using System.IO;
using System.Linq;
using flickr.uploader.domain;
using FlickrNet;
using Newtonsoft.Json;

namespace flickr.uploader.infrastructure
{
    public class FileService : IFileService
    {
        public IReadOnlyCollection<MediaFile> GetMediaFiles(string folder)
        {
            var photoExtensions = new[] { ".jpg", ".png" };
            var videoExtensions = new[] { ".mts", ".mp4" };

            var query = from filePath in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                        let fileInfo = new FileInfo(filePath)
                        let isPhoto = photoExtensions.Contains(fileInfo.Extension.ToLower())
                        let isVideo = videoExtensions.Contains(fileInfo.Extension.ToLower())
                        where isVideo || isPhoto
                        select new MediaFile {
                            FileName = fileInfo.Name,
                            Path = filePath,
                            MediaType = isPhoto ? MediaTypes.Photo : MediaTypes.Video,
                            Length = fileInfo.Length
                        };

            return query.ToList();
        }

        public T Deserialize<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Serialize<T>(T obj, string filePath)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}