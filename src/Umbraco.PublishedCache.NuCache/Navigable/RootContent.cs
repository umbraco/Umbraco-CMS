using Umbraco.Cms.Core.Xml.XPath;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal class RootContent : INavigableContent
{
    private static readonly RootContentType ContentType = new();
    private readonly int[] _childIds;

    public RootContent(IEnumerable<int> childIds) => _childIds = childIds.ToArray();

    public int Id => -1;

    public int ParentId => -1;

    public INavigableContentType Type => ContentType;

    public IList<int> ChildIds => _childIds;

    public object? Value(int index) =>

        // only id has a value
        index == 0 ? "-1" : null;

    private class RootContentType : INavigableContentType
    {
        public string Name => "root";

        public INavigableFieldType[] FieldTypes => NavigableContentType.BuiltinProperties;
    }
}
