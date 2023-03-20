namespace Umbraco.Cms.Core.Models.ContentEditing;

public class DomainsUpdateModel
{
    public string? DefaultIsoCode { get; set; }

    public required IEnumerable<DomainModel> Domains { get; set; }
}
