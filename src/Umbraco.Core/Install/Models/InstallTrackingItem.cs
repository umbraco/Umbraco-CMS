namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("Will no longer be required with the new backoffice API")]
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

    protected bool Equals(InstallTrackingItem other) => string.Equals(Name, other.Name);

    public override int GetHashCode() => Name.GetHashCode();
}
