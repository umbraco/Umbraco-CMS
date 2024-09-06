namespace Umbraco.Cms.Core.Models.Navigation;

public sealed class NavigationNode
{
    private List<NavigationNode> _children;

    public Guid Key { get; private set; }

    public NavigationNode? Parent { get; private set; }

    public IEnumerable<NavigationNode> Children => _children.AsEnumerable();

    public NavigationNode(Guid key)
    {
        Key = key;
        _children = new List<NavigationNode>();
    }

    public void AddChild(NavigationNode child)
    {
        child.Parent = this;
        _children.Add(child);
    }

    public void RemoveChild(NavigationNode child)
    {
        _children.Remove(child);
        child.Parent = null;
    }
}
