using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.TextFiles;

public abstract class TextFileResponseModelBase : TextFileViewModelBase, IResponseModel
{
    public string Path { get; set; } = string.Empty;
    public abstract string Type { get; }
}
