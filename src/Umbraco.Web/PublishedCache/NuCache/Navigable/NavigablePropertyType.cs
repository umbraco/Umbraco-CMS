using System;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    internal class NavigablePropertyType : INavigableFieldType
    {
        public NavigablePropertyType(string name, Func<object, string> xmlStringConverter = null)
        {
            Name = name;
            XmlStringConverter = xmlStringConverter;
        }

        public string Name { get; private set; }
        public Func<object, string> XmlStringConverter { get; private set; }
    }
}
