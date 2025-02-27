namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootRequestModel
{
    public required DynamicRootContextRequestModel Context { get; set; }

    public required DynamicRootQueryRequestModel Query { get; set; }
}
