namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootQueryStepRequestModel
{
    public required string Alias { get; set; }

    public required IEnumerable<Guid> DocumentTypeIds { get; set; }
}
