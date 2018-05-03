using System.Collections.Generic;

namespace flickr.uploader.domain
{
    public class Album
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<Photo> Photos { get; set; }
    }
}