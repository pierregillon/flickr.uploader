using Xunit;

namespace flickr.uploader.tests
{
    public class UploadFolderToFlickrCommandHandlerTests
    {
        [Fact]
        public void test()
        {
            var command = new UploadFolderToFlickrCommand {
                ApiKey = "a023233ad75a2e7ae38a1b1aa92ff751",
                ApiSecret = "abd048b37b9e44f9",
                PictureLocalFolder  = "Resources/",
                PhotoSetId = "test"
            };

            var handler = new UploadFolderToFlickrCommandHandler(new FlickrToken());

            handler.Handle(command);
        }
    }
}
