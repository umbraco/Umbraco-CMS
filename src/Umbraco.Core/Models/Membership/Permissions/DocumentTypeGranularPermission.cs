namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public class DocumentTypeGranularPermission : INodeGranularPermission
{
    public const string ContextType = "DocumentType";

    public required Guid Key { get; set; }

    public string Context => ContextType;

    public required string Permission { get; set; }

    protected bool Equals(DocumentTypeGranularPermission other) => Key.Equals(other.Key) && Permission == other.Permission;

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

        return Equals((DocumentTypeGranularPermission)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Key, Permission);
}
