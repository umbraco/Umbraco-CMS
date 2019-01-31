using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    internal class NavigableContentType : INavigableContentType
    {
        public static readonly INavigableFieldType[] BuiltinProperties;
        private readonly object _locko = new object();

        // called by the conditional weak table -- must be public
// ReSharper disable EmptyConstructor
        public NavigableContentType()
// ReSharper restore EmptyConstructor
        { }

        // note - PublishedContentType are immutable ie they do not _change_ when the actual IContentTypeComposition
        // changes, but they are replaced by a new instance, so our map here will clean itself automatically and
        // we don't have to manage cache - ConditionalWeakTable does not prevent keys from being GCed

        private static readonly ConditionalWeakTable<PublishedContentType, NavigableContentType> TypesMap
             = new ConditionalWeakTable<PublishedContentType,NavigableContentType>();

        public static NavigableContentType GetContentType(PublishedContentType contentType)
        {
            return TypesMap.GetOrCreateValue(contentType).EnsureInitialized(contentType);
        }

        static NavigableContentType()
        {
            BuiltinProperties = new INavigableFieldType[]
                    {
                        new NavigablePropertyType("nodeName"),
                        new NavigablePropertyType("parentId"),
                        new NavigablePropertyType("createDate", v => XmlConvert.ToString((DateTime)v, "yyyy-MM-ddTHH:mm:ss")),
                        new NavigablePropertyType("updateDate", v => XmlConvert.ToString((DateTime)v,  "yyyy-MM-ddTHH:mm:ss")),
                        new NavigablePropertyType("isDoc", v => XmlConvert.ToString((bool)v)),
                        new NavigablePropertyType("sortOrder"),
                        new NavigablePropertyType("level"),
                        new NavigablePropertyType("templateId"),
                        new NavigablePropertyType("writerId"),
                        new NavigablePropertyType("creatorId"),
                        new NavigablePropertyType("urlName"),
                        new NavigablePropertyType("isDraft", v => XmlConvert.ToString((bool)v))
                    };
        }

        private NavigableContentType EnsureInitialized(PublishedContentType contentType)
        {
            lock (_locko)
            {
                if (Name == null)
                {
                    Name = contentType.Alias;
                    FieldTypes = BuiltinProperties
                        .Union(contentType.PropertyTypes.Select(propertyType => new NavigablePropertyType(propertyType.Alias)))
                        .ToArray();
                }
            }
            return this;
        }

        public string Name { get; private set; }
        public INavigableFieldType[] FieldTypes { get; private set; }
    }
}
