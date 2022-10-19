// Copyright (c) Umbraco.
// See LICENSE for more details.

using CommandLine;

namespace JsonSchema
{
    internal class Options
    {
        [Option('m', "mainOutputFile", Required = false, HelpText = "Set path of the main output file.", Default = "../../../../Umbraco.Web.UI/appsettings-schema.json")]
        public string MainOutputFile { get; set; } = null!;

        [Option('f', "cmsOutputFile", Required = false, HelpText = "Set path of the cms output file.", Default = "../../../../Umbraco.Web.UI/appsettings-schema.umbraco.json")]
        public string CmsOutputFile { get; set; } = null!;
    }
}
