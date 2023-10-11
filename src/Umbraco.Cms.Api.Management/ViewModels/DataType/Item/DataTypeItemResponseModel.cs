using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class DataTypeItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }
    public override string Type => Constants.UdiEntityType.DataType;
}
