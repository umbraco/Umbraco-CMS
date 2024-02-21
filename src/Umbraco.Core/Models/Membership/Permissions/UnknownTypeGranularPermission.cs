namespace Umbraco.Cms.Core.Models.Membership.Permissions;

public class UnknownTypeGranularPermission : IGranularPermission
{
    public required string Context { get; set; }
    public required string Permission { get; set; }
}
