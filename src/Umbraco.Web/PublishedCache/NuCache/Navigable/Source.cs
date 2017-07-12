using System.Linq;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    internal class Source : INavigableSource
    {
        private readonly INavigableData _data;
        private readonly bool _preview;
        private readonly RootContent _root;

        public Source(INavigableData data, bool preview)
        {
            _data = data;
            _preview = preview;

            var contentAtRoot = data.GetAtRoot(preview);
            _root = new RootContent(contentAtRoot.Select(x => x.Id));
        }

        public INavigableContent Get(int id)
        {
            // wrap in a navigable content

            var content = _data.GetById(_preview, id);
            return content == null ? null : new NavigableContent(content);
        }

        public int LastAttributeIndex => NavigableContentType.BuiltinProperties.Length - 1;

        public INavigableContent Root => _root;
    }
}
