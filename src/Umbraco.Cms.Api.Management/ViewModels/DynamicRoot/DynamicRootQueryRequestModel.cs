namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootQueryRequestModel
{
    public required DynamicRootQueryOriginRequestModel Origin { get; set; }

    public required IEnumerable<DynamicRootQueryStepRequestModel> Steps { get; set; }
}
