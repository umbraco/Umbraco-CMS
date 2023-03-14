using Umbraco.Cms.Core.Xml.XPath;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal class NavigablePropertyType : INavigableFieldType
{
    public NavigablePropertyType(string name, Func<object, string>? xmlStringConverter = null)
    {
        Name = name;
        XmlStringConverter = xmlStringConverter;
    }

    public string Name { get; }

    public Func<object, string>? XmlStringConverter { get; }
}
