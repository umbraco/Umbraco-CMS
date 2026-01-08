using System.Collections.Concurrent;

namespace Umbraco.Cms.Core.Models.Navigation;

public sealed class NavigationNode
{
    private HashSet<Guid> _children;

    public Guid Key { get; private set; }

    public Guid ContentTypeKey { get; private set; }

    public int SortOrder { get; private set; }

    public Guid? Parent { get; private set; }

    public ISet<Guid> Children => _children;

    public NavigationNode(Guid key, Guid contentTypeKey, int sortOrder = 0)
    {
        Key = key;
        ContentTypeKey = contentTypeKey;
        SortOrder = sortOrder;
        _children = new HashSet<Guid>();
    }

    public void UpdateSortOrder(int newSortOrder) => SortOrder = newSortOrder;

    public void AddChild(ConcurrentDictionary<Guid, NavigationNode> navigationStructure, Guid childKey)
    {
        if (navigationStructure.TryGetValue(childKey, out NavigationNode? child) is false)
        {
            throw new KeyNotFoundException($"Item with key '{childKey}' was not found in the navigation structure.");
        }

        child.Parent = Key;

        // Add it as the last item
        child.SortOrder = _children.Count;

        _children.Add(childKey);
    }

    public void RemoveChild(ConcurrentDictionary<Guid, NavigationNode> navigationStructure, Guid childKey)
    {
        if (navigationStructure.TryGetValue(childKey, out NavigationNode? child) is false)
        {
            throw new KeyNotFoundException($"Item with key '{childKey}' was not found in the navigation structure.");
        }

        _children.Remove(childKey);
        child.Parent = null;
    }
}
