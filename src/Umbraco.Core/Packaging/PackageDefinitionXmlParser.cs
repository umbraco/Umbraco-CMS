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
    /// Parses the xml document contained in a compiled (zip) Umbraco package
    /// </summary>
    public class CompiledPackageXmlParser
    {
        public CompiledPackageXmlParser()
        {
            
        }

        public CompiledPackage ToCompiledPackage(XDocument xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (xml.Root == null) throw new ArgumentException(nameof(xml), "The xml document is invalid");
            if (xml.Root.Name != Constants.Packaging.UmbPackageNodeName) throw new FormatException("The xml document is invalid");

            var info = xml.Root.Element("info");
            if (info == null) throw new FormatException("The xml document is invalid");
            var package = xml.Element("package");
            if (package == null) throw new FormatException("The xml document is invalid");
            var author = package.Element("author");
            if (author == null) throw new FormatException("The xml document is invalid");
            var requirements = package.Element("requirements");
            if (requirements == null) throw new FormatException("The xml document is invalid");

            var def = new CompiledPackage
            {
                Name = package.Element("name")?.Value,
                Author = author.Element("name")?.Value,
                AuthorUrl = author.Element("website")?.Value,
                Version = package.Element("version")?.Value,
                Readme = info.Element("readme")?.Value,
                License = package.Element("license")?.Value,
                LicenseUrl = package.Element("license")?.AttributeValue<string>("url"),
                Url = package.Element("url")?.Value,
                IconUrl = package.Element("iconUrl")?.Value,
                UmbracoVersion = new Version((int)requirements.Element("major"), (int)requirements.Element("minor"), (int)requirements.Element("patch")),
                UmbracoVersionRequirementsType = Enum<RequirementsType>.Parse(requirements.AttributeValue<string>("type")),
                Control = package.Element("control")?.Value,

                Files = xml.Root.Element("files")?.Elements("files")?.Select(x => new CompiledPackageFile
                    {
                        UniqueFileName = x.Element("guid")?.Value,
                        OriginalName = x.Element("orgPath")?.Value,
                        OriginalPath = x.Element("orgName")?.Value
                    }).ToList() ?? new List<CompiledPackageFile>(),
                
            };

            return def;
        }

    }

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
                FolderId = xml.AttributeValue<Guid>("folder"),
                PackagePath = xml.AttributeValue<string>("packagePath") ?? string.Empty,
                Version = xml.AttributeValue<string>("version") ?? string.Empty,
                Url = xml.AttributeValue<string>("url") ?? string.Empty,
                PackageId = xml.AttributeValue<Guid>("packageGuid"),
                IconUrl = xml.AttributeValue<string>("iconUrl") ?? string.Empty,
                UmbracoVersion = xml.AttributeValue<Version>("umbVersion"),
                License = xml.Element("license")?.Value ?? string.Empty,
                LicenseUrl = xml.Element("license")?.AttributeValue<string>("url") ?? string.Empty,
                Author = xml.Element("author")?.Value ?? string.Empty,
                AuthorUrl = xml.Element("author")?.AttributeValue<string>("url") ?? string.Empty,
                Readme = xml.Element("readme")?.Value ?? string.Empty,
                Actions = xml.Element("actions")?.ToString(SaveOptions.None) ?? "<actions></actions>", //take the entire outer xml value
                ContentNodeId = xml.Element("content")?.AttributeValue<string>("nodeId") ?? string.Empty,
                ContentLoadChildNodes = xml.Element("content")?.AttributeValue<bool>("loadChildNodes") ?? false,
                Macros = xml.Element("macros")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Templates = xml.Element("templates")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Stylesheets = xml.Element("stylesheets")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DocumentTypes = xml.Element("documentTypes")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Languages = xml.Element("languages")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DictionaryItems = xml.Element("dictionaryitems")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                DataTypes = xml.Element("datatypes")?.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
                Files = xml.Element("files")?.Elements("file").Select(x => x.Value).ToList() ?? new List<string>(),
                Control = xml.Element("loadcontrol")?.Value ?? string.Empty
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
                new XAttribute("folder", def.FolderId),
                new XAttribute("packagePath", def.PackagePath ?? string.Empty),
                new XAttribute("iconUrl", def.IconUrl ?? string.Empty),
                new XAttribute("umbVersion", def.UmbracoVersion),
                new XAttribute("packageGuid", def.PackageId),

                new XElement("license",
                    new XCData(def.License ?? string.Empty),
                    new XAttribute("url", def.LicenseUrl ?? string.Empty)),

                new XElement("author",
                    new XCData(def.Author ?? string.Empty),
                    new XAttribute("url", def.AuthorUrl ?? string.Empty)),

                new XElement("readme", new XCData(def.Readme ?? string.Empty)),
                actionsXml,
                new XElement("datatypes", string.Join(",", def.DataTypes ?? Array.Empty<string>())),

                new XElement("content",
                    new XAttribute("nodeId", def.ContentNodeId),
                    new XAttribute("loadChildNodes", def.ContentLoadChildNodes)),

                new XElement("templates", string.Join(",", def.Templates ?? Array.Empty<string>())),
                new XElement("stylesheets", string.Join(",", def.Stylesheets ?? Array.Empty<string>())),
                new XElement("documentTypes", string.Join(",", def.DocumentTypes ?? Array.Empty<string>())),
                new XElement("macros", string.Join(",", def.Macros ?? Array.Empty<string>())),
                new XElement("files", (def.Files ?? Array.Empty<string>()).Where(x => !x.IsNullOrWhiteSpace()).Select(x => new XElement("file", x))),
                new XElement("languages", string.Join(",", def.Languages ?? Array.Empty<string>())),
                new XElement("dictionaryitems", string.Join(",", def.DictionaryItems ?? Array.Empty<string>())),
                new XElement("loadcontrol", def.Control ?? string.Empty)); //fixme: no more loadcontrol, needs to be an angular view

            return packageXml;
        }
    }
}
