using System;
using System.Xml.Linq;

namespace Umbraco.Core.Models.Packaging
{
    public class CompiledPackageFile
    {
        public static CompiledPackageFile Create(XElement xml)
        {
            if (xml.Name.LocalName != "file")
                throw new ArgumentException("The xml isn't formatted correctly, a file element is defined by <file>", nameof(xml));
            return new CompiledPackageFile
            {
                UniqueFileName = xml.Element("guid")?.Value,
                OriginalName = xml.Element("orgName")?.Value,
                OriginalPath = xml.Element("orgPath")?.Value
            };
        }

        public string OriginalPath { get; set; }
        public string UniqueFileName { get; set; }
        public string OriginalName { get; set; }

    }
}