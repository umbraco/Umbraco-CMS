using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
    internal class FileSystemProvider
    {
        public const string Media = "media";
    }
}
