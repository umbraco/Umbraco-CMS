using System.Runtime.CompilerServices;
using System.Xml;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Xml.XPath;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal class NavigableContentType : INavigableContentType
{
    public static readonly INavigableFieldType[] BuiltinProperties;

    // note - PublishedContentType are immutable ie they do not _change_ when the actual IContentTypeComposition
    // changes, but they are replaced by a new instance, so our map here will clean itself automatically and
    // we don't have to manage cache - ConditionalWeakTable does not prevent keys from being GCed
    private static readonly ConditionalWeakTable<IPublishedContentType, NavigableContentType> TypesMap = new();

    private readonly object _locko = new();

    static NavigableContentType() =>
        BuiltinProperties = new INavigableFieldType[]
        {
            new NavigablePropertyType("nodeName"), new NavigablePropertyType("parentId"),
            new NavigablePropertyType("createDate", v => XmlConvert.ToString((DateTime)v, "yyyy-MM-ddTHH:mm:ss")),
            new NavigablePropertyType("updateDate", v => XmlConvert.ToString((DateTime)v, "yyyy-MM-ddTHH:mm:ss")),
            new NavigablePropertyType("isDoc", v => XmlConvert.ToString((bool)v)),
            new NavigablePropertyType("sortOrder"), new NavigablePropertyType("level"),
            new NavigablePropertyType("templateId"), new NavigablePropertyType("writerId"),
            new NavigablePropertyType("creatorId"), new NavigablePropertyType("urlName"),
            new NavigablePropertyType("isDraft", v => XmlConvert.ToString((bool)v)),
        };

    // called by the conditional weak table -- must be public
    // ReSharper disable EmptyConstructor
#pragma warning disable CS8618
    public NavigableContentType()
#pragma warning restore CS8618

    // ReSharper restore EmptyConstructor
    {
    }

    public string Name { get; private set; }

    public INavigableFieldType[] FieldTypes { get; private set; }

    public static NavigableContentType GetContentType(IPublishedContentType contentType) =>
        TypesMap.GetOrCreateValue(contentType).EnsureInitialized(contentType);

    private NavigableContentType EnsureInitialized(IPublishedContentType contentType)
    {
        lock (_locko)
        {
            if (Name == null)
            {
                Name = contentType.Alias;
                FieldTypes = BuiltinProperties
                    .Union(contentType.PropertyTypes.Select(propertyType =>
                        new NavigablePropertyType(propertyType.Alias)))
                    .ToArray();
            }
        }

        return this;
    }
}
