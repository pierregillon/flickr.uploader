namespace flickr.uploader.domain
{
    public class MediaFile
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public MediaTypes MediaType { get; set; }
        public long Length { get; set; }
    }
}