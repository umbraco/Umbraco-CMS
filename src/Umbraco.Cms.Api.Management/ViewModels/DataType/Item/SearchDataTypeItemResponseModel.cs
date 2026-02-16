using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

public class SearchDataTypeItemResponseModel : DataTypeItemResponseModel
{
    public IEnumerable<SearchResultAncestorModel> Ancestors { get; set; } = [];
}
