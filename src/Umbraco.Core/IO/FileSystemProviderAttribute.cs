using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class FileSystemProviderAttribute : Attribute
    {
        public string Alias { get; set; }

        public FileSystemProviderAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
