﻿namespace Umbraco.Core.Packaging
{
    public class PackageMetaData
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }
        public int ReqMajor { get; set; }
        public int ReqMinor { get; set; }
        public int ReqPatch { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUrl { get; set; }
        public string Readme { get; set; }
        public string Control { get; set; }
    }
}