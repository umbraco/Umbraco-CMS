using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    internal class RootContent : INavigableContent
    {
        private static readonly RootContentType ContentType = new RootContentType();
        private readonly int[] _childIds;

        public RootContent(IEnumerable<int> childIds)
        {
            _childIds = childIds.ToArray();
        }

        public int Id => -1;

        public int ParentId => -1;

        public INavigableContentType Type => ContentType;

        public IList<int> ChildIds => _childIds;

        public object Value(int index)
        {
            // only id has a value
            return index == 0 ? "-1" : null;
        }

        private class RootContentType : INavigableContentType
        {
            public string Name => "root";

            public INavigableFieldType[] FieldTypes => NavigableContentType.BuiltinProperties;
        }
    }
}
