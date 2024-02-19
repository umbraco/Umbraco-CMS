namespace Umbraco.Cms.Core.Models.Membership;

public interface IGranularPermission
{
    public Guid Key { get; set; }
    public string Permission { get; set; }
}

public class GranularPermission : IGranularPermission
{
    public required Guid Key { get; set; }
    public required string Permission { get; set; }
}
