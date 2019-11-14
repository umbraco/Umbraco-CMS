using System;
using System.Xml.Linq;

namespace Umbraco.Core.Models.Packaging
{
    public class CompiledPackageDocument
    {
        public static CompiledPackageDocument Create(XElement xml)
        {
            if (xml.Name.LocalName != "DocumentSet")
                throw new ArgumentException("The xml isn't formatted correctly, a document element is defined by <DocumentSet>", nameof(xml));
            return new CompiledPackageDocument
            {
                XmlData = xml,
                ImportMode = xml.AttributeValue<string>("importMode")
            };
        }

        public string ImportMode { get; set; } //this is never used

        /// <summary>
        /// The serialized version of the content
        /// </summary>
        public XElement XmlData { get; set; }
    }
}