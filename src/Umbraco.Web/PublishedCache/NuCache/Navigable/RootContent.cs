using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    class RootContent : INavigableContent
    {
        private static readonly RootContentType ContentType = new RootContentType();
        private readonly int[] _childIds;

        public RootContent(IEnumerable<int> childIds)
        {
            _childIds = childIds.ToArray();
        }

        public int Id
        {
            get { return -1; }
        }

        public int ParentId
        {
            get { return -1; }
        }

        public INavigableContentType Type
        {
            get { return ContentType; }
        }

        public IList<int> ChildIds
        {
            get { return _childIds; }
        }

        public object Value(int index)
        {
            // only id has a value
            return index == 0 ? "-1" : null;
        }

        class RootContentType : INavigableContentType
        {
            public string Name
            {
                get { return "root"; }
            }

            public INavigableFieldType[] FieldTypes
            {
                get { return NavigableContentType.BuiltinProperties; }
            }
        }
    }
}
