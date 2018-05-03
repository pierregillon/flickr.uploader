namespace flickr.uploader.domain {
    public class AuthenticatedCommand
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
    }
}