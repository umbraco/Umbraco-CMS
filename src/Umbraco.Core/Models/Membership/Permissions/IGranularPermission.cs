namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public interface IGranularPermission
{
    public string Context { get; }

    public Guid? Key => null;

    public string Permission { get; set; }
}
