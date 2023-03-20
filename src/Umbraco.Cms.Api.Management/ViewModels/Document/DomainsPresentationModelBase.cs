namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public abstract class DomainsPresentationModelBase
{
    public string? DefaultIsoCode { get; set; }

    public required IEnumerable<DomainPresentationModel> Domains { get; set; }
}
