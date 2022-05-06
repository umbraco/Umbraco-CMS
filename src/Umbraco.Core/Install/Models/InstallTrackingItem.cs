namespace Umbraco.Cms.Core.Install.Models;

public class InstallTrackingItem
{
    public InstallTrackingItem(string name, int serverOrder)
    {
        Name = name;
        ServerOrder = serverOrder;
        AdditionalData = new Dictionary<string, object>();
    }

    public string Name { get; set; }
    public int ServerOrder { get; set; }
    public bool IsComplete { get; set; }
    public IDictionary<string, object> AdditionalData { get; set; }

    protected bool Equals(InstallTrackingItem other) => string.Equals(Name, other.Name);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((InstallTrackingItem)obj);
    }

    public override int GetHashCode() => Name.GetHashCode();
}
