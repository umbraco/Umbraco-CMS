namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

public class DynamicRootResponseModel
{
    public IEnumerable<Guid> Roots { get; set; } = Enumerable.Empty<Guid>();
}
