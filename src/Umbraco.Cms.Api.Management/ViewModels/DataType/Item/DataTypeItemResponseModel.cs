using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeItemResponseModel : NamedItemResponseModelBase
{
    public string? EditorUiAlias { get; set; }

    public string EditorAlias { get; set; } = string.Empty;

    public bool IsDeletable { get; set; }
}
