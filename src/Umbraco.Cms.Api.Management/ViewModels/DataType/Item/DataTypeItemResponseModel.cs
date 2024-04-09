using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeItemResponseModel : NamedItemResponseModelBase
{
    public string? EditorUiAlias { get; set; }

    public bool IsDeletable { get; set; }
}
