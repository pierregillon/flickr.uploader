﻿using CommandLine;

namespace flickr.uploader.recursive
{
    public class Options
    {
        [Option("apikey", Required = true, HelpText = "The Flickr api key")]
        public string ApiKey { get; set; }

        [Option("apisecret", Required = true, HelpText = "The Flickr api secret")]
        public string ApiSecret { get; set; }

        [Option("folder", Required = true, HelpText = "The folder to parse and to upload pictures")]
        public string LocalFolder { get; set; }
    }
}
