using Umbraco.Cms.Api.Management.ViewModels.TextFiles;

namespace Umbraco.Cms.Api.Management.ViewModels.PartialView;

public class UpdatePartialViewRequestModel : TextFileViewModelBase
{
    public required string ExistingPath { get; set; }
}
