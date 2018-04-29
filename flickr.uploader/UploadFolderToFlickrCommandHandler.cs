namespace flickr.uploader
{
    public class UploadFolderToFlickrCommandHandler
    {
        private readonly FlickApi _flickApi;

        public UploadFolderToFlickrCommandHandler(FlickApi flickApi)
        {
            _flickApi = flickApi;
        }

        public void Handle(UploadFolderToFlickrCommand command) { }
    }
}