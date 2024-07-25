namespace Umbraco.Cms.Core.Models.Navigation;

public class NavigationNode
{
    public Guid Key { get; private set; }

    public NavigationNode? Parent { get; private set; }

    public List<NavigationNode> Children { get; private set; }


    public NavigationNode(Guid key)
    {
        Key = key;
        Children = new List<NavigationNode>();
    }

    public void AddChild(NavigationNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }
}
