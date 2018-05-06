using System.Diagnostics;
using FlickrNet;

namespace flickr.uploader.domain.Authenticate
{
    public class AuthenticateCommandHandler : ICommandHandler<AuthenticateCommand>
    {
        private const string TOKEN_FILE_PATH = ".flickr";

        private readonly IConsole _console;
        private readonly IFileService _fileService;
        private readonly IFlickrService _flickrService;

        // ----- Constructors
        public AuthenticateCommandHandler(
            IConsole console,
            IFlickrService flickrService,
            IFileService fileService)
        {
            _console = console;
            _flickrService = flickrService;
            _fileService = fileService;
        }

        // ----- Public methods
        public void Handle(AuthenticateCommand command)
        {
            var flickr = GetAuthentifiedFlickrClient(command.ApiKey, command.ApiSecret);
            _flickrService.Authenticate(flickr);
        }

        // ----- Internal logics
        private Flickr GetAuthentifiedFlickrClient(string apiKey, string apiSecret)
        {
            if (!_fileService.Exists(TOKEN_FILE_PATH)) {
                return CreateNewFlickrClient(apiKey, apiSecret, TOKEN_FILE_PATH);
            }
            try {
                return CreateFlickrClientFromExistingToken(apiKey, apiSecret, TOKEN_FILE_PATH);
            }
            catch (OAuthException) {
                return CreateNewFlickrClient(apiKey, apiSecret, TOKEN_FILE_PATH);
            }
        }
        private Flickr CreateFlickrClientFromExistingToken(string apiKey, string apiSecret, string tokenFilePath)
        {
            return _console.StartOperation(
                $"* Reading token from '{tokenFilePath}' file ... ",
                () => {
                    var accesToken = _fileService.Deserialize<OAuthAccessToken>(tokenFilePath);
                    var flickr = new Flickr(apiKey, apiSecret) {
                        OAuthAccessToken = accesToken.Token,
                        OAuthAccessTokenSecret = accesToken.TokenSecret
                    };
                    flickr.AuthOAuthCheckToken();
                    return flickr;
                });
        }
        private Flickr CreateNewFlickrClient(string apiKey, string apiSecret, string tokenFilePath)
        {
            var flickr = new Flickr(apiKey, apiSecret);
            var accesToken = CreateNewAccessToken(flickr);
            flickr.OAuthAccessToken = accesToken.Token;
            flickr.OAuthAccessTokenSecret = accesToken.TokenSecret;
            _fileService.Serialize(accesToken, tokenFilePath);
            _console.WriteLine($"* Access token saved in '{tokenFilePath}' file.");
            return flickr;
        }
        private OAuthAccessToken CreateNewAccessToken(Flickr flickr)
        {
            _console.WriteLine("* Creation of a new token.");
            var requestToken = flickr.OAuthGetRequestToken("oob");
            var url = flickr.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Delete);
            _console.Write("* Opening browser ... ");
            Process.Start(url)?.WaitForExit();
            _console.WriteLine("[DONE]");

            while (true) {
                try {
                    _console.Write("* Type the Flickr verifier code : ");
                    var verifier = _console.ReadLine();
                    var accessToken = flickr.OAuthGetAccessToken(requestToken, verifier);
                    _console.WriteLine("* New access token acquired.");
                    return accessToken;
                }
                catch (OAuthException) {
                    _console.WriteLine("=> ERROR");
                }
            }
        }
    }
}