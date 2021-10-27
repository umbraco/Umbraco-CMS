using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    /// <summary>
    /// Converts a <see cref="PackageDefinition"/> to and from XML
    /// </summary>
    public class PackageDefinitionXmlParser
    {
        private readonly ILogger _logger;

        public PackageDefinitionXmlParser(ILogger logger)
        {
            _logger = logger;
        }

        public PackageDefinition ToPackageDefinition(XElement xml)
        {
            if (xml == null) return null;

            var retVal = new PackageDefinition
            {
                Id = xml.AttributeValue<int>("id"),
                Name = xml.AttributeValue<string>("name") ?? string.Empty,
                PackagePath = xml.AttributeValue<string>("packagePath") ?? string.Empty,
                Version = xml.AttributeValue<string>("version") ?? string.Empty,
                Url = xml.AttributeValue<string>("url") ?? string.Empty,
                PackageId = xml.AttributeValue<Guid>("packageGuid"),
                IconUrl = xml.AttributeValue<string>("iconUrl") ?? string.Empty,
                UmbracoVersion = xml.AttributeValue<Version>("umbVersion"),
                PackageView = xml.AttributeValue<string>("view") ?? string.Empty,
                License = xml.Element("license")?.Value ?? string.Empty,
                LicenseUrl = xml.Element("license")?.AttributeValue<string>("url") ?? string.Empty,
                Author = xml.Element("author")?.Value ?? string.Empty,
                AuthorUrl = xml.Element("author")?.AttributeValue<string>("url") ?? string.Empty,
                Contributors = xml.Element("contributors")?.Elements("contributor").Select(x => x.Value).ToList() ?? new List<string>(),
                Readme = xml.Element("readme")?.Value ?? string.Empty,
                Actions = xml.Element("actions")?.ToString(SaveOptions.None) ?? "<actions></actions>", //take the entire outer xml value
                ContentNodeId = xml.Element("content")?.AttributeValue<string>("nodeId") ?? string.Empty,
                ContentLoadChildNodes = xml.Element("content")?.AttributeValue<bool>("loadChildNodes") ?? false,
                Macros = xml.Element("macros")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Templates = xml.Element("templates")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Stylesheets = xml.Element("stylesheets")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DocumentTypes = xml.Element("documentTypes")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Languages = xml.Element("languages")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DictionaryItems = xml.Element("dictionaryitems")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DataTypes = xml.Element("datatypes")?.Value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Files = xml.Element("files")?.Elements("file").Select(x => x.Value).ToList() ?? new List<string>()
            };

            return retVal;
        }

        public XElement ToXml(PackageDefinition def)
        {
            var actionsXml = new XElement("actions");
            try
            {
                actionsXml = XElement.Parse(def.Actions);
            }
            catch (Exception e)
            {
                _logger.Warn<PackagesRepository>(e, "Could not add package actions to the package xml definition, the xml did not parse");
            }

            var packageXml = new XElement("package",
                new XAttribute("id", def.Id),
                new XAttribute("version", def.Version ?? string.Empty),
                new XAttribute("url", def.Url ?? string.Empty),
                new XAttribute("name", def.Name ?? string.Empty),
                new XAttribute("packagePath", def.PackagePath ?? string.Empty),
                new XAttribute("iconUrl", def.IconUrl ?? string.Empty),
                new XAttribute("umbVersion", def.UmbracoVersion),
                new XAttribute("packageGuid", def.PackageId),
                new XAttribute("view", def.PackageView ?? string.Empty),

                new XElement("license",
                    new XCData(def.License ?? string.Empty),
                    new XAttribute("url", def.LicenseUrl ?? string.Empty)),

                new XElement("author",
                    new XCData(def.Author ?? string.Empty),
                    new XAttribute("url", def.AuthorUrl ?? string.Empty)),

                new XElement("contributors", (def.Contributors ?? Array.Empty<string>()).Where(x => !x.IsNullOrWhiteSpace()).Select(x => new XElement("contributor", x))),

                new XElement("readme", new XCData(def.Readme ?? string.Empty)),
                actionsXml,
                new XElement("datatypes", string.Join(",", def.DataTypes ?? Array.Empty<string>())),

                new XElement("content",
                    new XAttribute("nodeId", def.ContentNodeId ?? string.Empty),
                    new XAttribute("loadChildNodes", def.ContentLoadChildNodes)),

                new XElement("templates", string.Join(",", def.Templates ?? Array.Empty<string>())),
                new XElement("stylesheets", string.Join(",", def.Stylesheets ?? Array.Empty<string>())),
                new XElement("documentTypes", string.Join(",", def.DocumentTypes ?? Array.Empty<string>())),
                new XElement("macros", string.Join(",", def.Macros ?? Array.Empty<string>())),
                new XElement("files", (def.Files ?? Array.Empty<string>()).Where(x => !x.IsNullOrWhiteSpace()).Select(x => new XElement("file", x))),
                new XElement("languages", string.Join(",", def.Languages ?? Array.Empty<string>())),
                new XElement("dictionaryitems", string.Join(",", def.DictionaryItems ?? Array.Empty<string>()))); 

            return packageXml;
        }

        
    }
}
