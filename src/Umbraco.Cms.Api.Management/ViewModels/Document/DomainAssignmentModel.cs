namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public sealed class DomainAssignmentModel
{
    public required string DomainName { get; set; }

    public required ReferenceByIdModel Content { get; set; }
}
