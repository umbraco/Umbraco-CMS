namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public abstract class DomainsModelBase
{
    public string? DefaultIsoCode { get; set; }

    public required IEnumerable<DomainModel> Domains { get; set; }
}
