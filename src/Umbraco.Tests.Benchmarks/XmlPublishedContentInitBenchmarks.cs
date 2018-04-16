using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BenchmarkDotNet.Attributes;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.Benchmarks.Config;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class XmlPublishedContentInitBenchmarks
    {
        public XmlPublishedContentInitBenchmarks()
        {
            _xml10 = Build(10);
            _xml100 = Build(100);
            _xml1000 = Build(1000);
            _xml10000 = Build(10000);
        }

        private readonly string[] _intAttributes = { "id", "parentID", "nodeType", "level", "writerID", "creatorID", "template", "sortOrder", "isDoc", "isDraft" };
        private readonly string[] _strAttributes = { "nodeName", "urlName", "writerName", "creatorName", "path" };
        private readonly string[] _dateAttributes = { "createDate", "updateDate" };
        private readonly string[] _guidAttributes = { "key", "version" };

        private XmlDocument Build(int children)
        {
            var xml = new XmlDocument();
            var root = Build(xml, "Home", 10);
            for (int i = 0; i < children; i++)
            {
                var child = Build(xml, "child" + i, 10);
                root.AppendChild(child);
            }
            xml.AppendChild(root);
            return xml;
        }

        private XmlElement Build(XmlDocument xml, string name, int propertyCount)
        {
            var random = new Random();
            var content = xml.CreateElement(name);
            foreach (var p in _intAttributes)
            {
                var a = xml.CreateAttribute(p);
                a.Value = random.Next(1, 9).ToInvariantString();
                content.Attributes.Append(a);
            }
            foreach (var p in _strAttributes)
            {
                var a = xml.CreateAttribute(p);
                a.Value = Guid.NewGuid().ToString();
                content.Attributes.Append(a);
            }
            foreach (var p in _guidAttributes)
            {
                var a = xml.CreateAttribute(p);
                a.Value = Guid.NewGuid().ToString();
                content.Attributes.Append(a);
            }
            foreach (var p in _dateAttributes)
            {
                var a = xml.CreateAttribute(p);
                a.Value = DateTime.Now.ToString("o");
                content.Attributes.Append(a);
            }

            for (int i = 0; i < propertyCount; i++)
            {
                var prop = xml.CreateElement("prop" + i);
                var cdata = xml.CreateCDataSection(string.Join("", Enumerable.Range(0, 10).Select(x => Guid.NewGuid().ToString())));
                prop.AppendChild(cdata);
                content.AppendChild(prop);
            }

            return content;
        }

        private readonly XmlDocument _xml10;
        private readonly XmlDocument _xml100;
        private readonly XmlDocument _xml1000;
        private readonly XmlDocument _xml10000;

        //out props
        int id, nodeType, level, writerId, creatorId, template, sortOrder;
        Guid key, version;
        string name, urlName, writerName, creatorName, docTypeAlias, path;
        bool isDraft;
        DateTime createDate, updateDate;
        PublishedContentType publishedContentType;
        Dictionary<string, IPublishedProperty> properties;

        [Benchmark(Baseline = true, OperationsPerInvoke = 10)]
        public void Original_10_Children()
        {
            OriginalInitializeNode(_xml10.DocumentElement, false, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out version, out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Original_100_Children()
        {
            OriginalInitializeNode(_xml100.DocumentElement, false, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out version, out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Original_1000_Children()
        {
            OriginalInitializeNode(_xml1000.DocumentElement, false, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out version, out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Original_10000_Children()
        {
            OriginalInitializeNode(_xml10000.DocumentElement, false, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out version, out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Enhanced_10_Children()
        {
            XmlPublishedContent.InitializeNode(null, _xml10.DocumentElement, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties, GetPublishedContentType);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Enhanced_100_Children()
        {
            XmlPublishedContent.InitializeNode(null, _xml100.DocumentElement, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties, GetPublishedContentType);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Enhanced_1000_Children()
        {
            XmlPublishedContent.InitializeNode(null, _xml1000.DocumentElement, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties, GetPublishedContentType);
        }

        [Benchmark(OperationsPerInvoke = 10)]
        public void Enhanced_10000_Children()
        {
            XmlPublishedContent.InitializeNode(null, _xml10000.DocumentElement, false,
                out id, out key, out template, out sortOrder, out name, out writerName, out urlName,
                out creatorName, out creatorId, out writerId, out docTypeAlias, out nodeType, out path,
                out createDate, out updateDate, out level, out isDraft, out publishedContentType,
                out properties, GetPublishedContentType);
        }


        internal static void OriginalInitializeNode(XmlNode xmlNode, bool legacy, bool isPreviewing,
            out int id, out Guid key, out int template, out int sortOrder, out string name, out string writerName, out string urlName,
            out string creatorName, out int creatorId, out int writerId, out string docTypeAlias, out int docTypeId, out string path,
            out Guid version, out DateTime createDate, out DateTime updateDate, out int level, out bool isDraft,
            out PublishedContentType contentType, out Dictionary<string, IPublishedProperty> properties)
        {
            //initialize the out params with defaults:
            writerName = null;
            docTypeAlias = null;
            id = template = sortOrder = template = creatorId = writerId = docTypeId = level = default(int);
            key = version = default(Guid);
            name = writerName = urlName = creatorName = docTypeAlias = path = null;
            createDate = updateDate = default(DateTime);
            isDraft = false;
            contentType = null;
            properties = null;

            //return if this is null
            if (xmlNode == null)
            {
                return;
            }

            if (xmlNode.Attributes != null)
            {
                id = int.Parse(xmlNode.Attributes.GetNamedItem("id").Value);
                if (xmlNode.Attributes.GetNamedItem("key") != null) // because, migration
                    key = Guid.Parse(xmlNode.Attributes.GetNamedItem("key").Value);
                if (xmlNode.Attributes.GetNamedItem("template") != null)
                    template = int.Parse(xmlNode.Attributes.GetNamedItem("template").Value);
                if (xmlNode.Attributes.GetNamedItem("sortOrder") != null)
                    sortOrder = int.Parse(xmlNode.Attributes.GetNamedItem("sortOrder").Value);
                if (xmlNode.Attributes.GetNamedItem("nodeName") != null)
                    name = xmlNode.Attributes.GetNamedItem("nodeName").Value;
                if (xmlNode.Attributes.GetNamedItem("writerName") != null)
                    writerName = xmlNode.Attributes.GetNamedItem("writerName").Value;
                if (xmlNode.Attributes.GetNamedItem("urlName") != null)
                    urlName = xmlNode.Attributes.GetNamedItem("urlName").Value;
                // Creatorname is new in 2.1, so published xml might not have it!
                try
                {
                    creatorName = xmlNode.Attributes.GetNamedItem("creatorName").Value;
                }
                catch
                {
                    creatorName = writerName;
                }

                //Added the actual userID, as a user cannot be looked up via full name only...
                if (xmlNode.Attributes.GetNamedItem("creatorID") != null)
                    creatorId = int.Parse(xmlNode.Attributes.GetNamedItem("creatorID").Value);
                if (xmlNode.Attributes.GetNamedItem("writerID") != null)
                    writerId = int.Parse(xmlNode.Attributes.GetNamedItem("writerID").Value);

                if (legacy)
                {
                    if (xmlNode.Attributes.GetNamedItem("nodeTypeAlias") != null)
                        docTypeAlias = xmlNode.Attributes.GetNamedItem("nodeTypeAlias").Value;
                }
                else
                {
                    docTypeAlias = xmlNode.Name;
                }

                if (xmlNode.Attributes.GetNamedItem("nodeType") != null)
                    docTypeId = int.Parse(xmlNode.Attributes.GetNamedItem("nodeType").Value);
                if (xmlNode.Attributes.GetNamedItem("path") != null)
                    path = xmlNode.Attributes.GetNamedItem("path").Value;
                if (xmlNode.Attributes.GetNamedItem("version") != null)
                    version = new Guid(xmlNode.Attributes.GetNamedItem("version").Value);
                if (xmlNode.Attributes.GetNamedItem("createDate") != null)
                    createDate = DateTime.Parse(xmlNode.Attributes.GetNamedItem("createDate").Value);
                if (xmlNode.Attributes.GetNamedItem("updateDate") != null)
                    updateDate = DateTime.Parse(xmlNode.Attributes.GetNamedItem("updateDate").Value);
                if (xmlNode.Attributes.GetNamedItem("level") != null)
                    level = int.Parse(xmlNode.Attributes.GetNamedItem("level").Value);

                isDraft = (xmlNode.Attributes.GetNamedItem("isDraft") != null);
            }

            // load data

            var dataXPath = legacy ? "data" : "* [not(@isDoc)]";
            var nodes = xmlNode.SelectNodes(dataXPath);

            contentType = GetPublishedContentType(PublishedItemType.Content, docTypeAlias);

            var propertyNodes = new Dictionary<string, XmlNode>();
            if (nodes != null)
                foreach (XmlNode n in nodes)
                {
                    var attrs = n.Attributes;
                    if (attrs == null) continue;
                    var alias = legacy
                        ? attrs.GetNamedItem("alias").Value
                        : n.Name;
                    propertyNodes[alias.ToLowerInvariant()] = n;
                }
            properties = contentType.PropertyTypes.Select(p =>
            {
                XmlNode n;
                return propertyNodes.TryGetValue(p.Alias.ToLowerInvariant(), out n)
                    ? new XmlPublishedProperty(p, null, isPreviewing, n)
                    : new XmlPublishedProperty(p, null, isPreviewing);
            }).Cast<IPublishedProperty>().ToDictionary(
                x => x.Alias,
                x => x,
                StringComparer.OrdinalIgnoreCase);
        }

        private static PublishedContentType GetPublishedContentType(PublishedItemType type, string alias)
        {
            var dataType = new DataType(new VoidEditor(Mock.Of<ILogger>())) { Id = 1 };

            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns<int>(id => id == 1 ? dataType : null);
            Mock.Get(dataTypeService)
                .Setup(x => x.GetAll())
                .Returns(new[] { dataType });

            var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
            return factory.CreateContentType(0, alias, new string[] {},
                new List<PublishedPropertyType>(Enumerable.Range(0, 10).Select(x => factory.CreatePropertyType("prop" + x, 1))));
        }
    }
}
