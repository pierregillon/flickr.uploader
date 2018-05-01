using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        let extension = Path.GetExtension(filePath)
                        let isPhoto = photoExtensions.Contains(extension.ToLower())
                        //let isVideo = videoExtensions.Contains(extension.ToLower())
                        where /*isVideo ||*/ isPhoto
                        select new MediaFile {
                            FileName = Path.GetFileName(filePath),
                            Path = filePath,
                            MediaType = isPhoto ? MediaTypes.Photo : MediaTypes.Video
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
    }
}