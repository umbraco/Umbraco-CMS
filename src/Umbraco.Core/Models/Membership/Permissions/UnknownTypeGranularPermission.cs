namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public class UnknownTypeGranularPermission : IGranularPermission
{
    public required string Context { get; set; }

    public required string Permission { get; set; }

    protected bool Equals(UnknownTypeGranularPermission other) => Context == other.Context && Permission == other.Permission;

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

        return Equals((UnknownTypeGranularPermission)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Context, Permission);
}
