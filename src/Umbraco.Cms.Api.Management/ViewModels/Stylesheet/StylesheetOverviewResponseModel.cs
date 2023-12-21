using Umbraco.Cms.Api.Management.ViewModels.FileSystem;

namespace Umbraco.Cms.Api.Management.ViewModels.Stylesheet;

public class StylesheetOverviewResponseModel : FileSystemItemViewModelBase
{
    public required string Name { get; set; }
}
