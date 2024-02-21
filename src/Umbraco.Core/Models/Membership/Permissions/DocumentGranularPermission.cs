namespace Umbraco.Cms.Core.Models.Membership.Permissions;


public class DocumentGranularPermission : INodeGranularPermission
{
    public const string ContextType = "Document";
    public required Guid Key { get; set; }
    public string Context => ContextType;
    public required string Permission { get; set; }
}
