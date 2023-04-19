using Umbraco.Cms.Api.Management.ViewModels.TextFiles;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView;

public class PartialViewResponseModel : TextFileViewModelBase
{
    public string Path { get; set; } = string.Empty;
}
