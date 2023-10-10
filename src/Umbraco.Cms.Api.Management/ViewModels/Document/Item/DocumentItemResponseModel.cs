using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Item;

public class DocumentItemResponseModel : ItemResponseModelBase
{
    public string? Icon { get; set; }

    public Guid ContentTypeId { get; set; }
    public override string Type => Constants.UdiEntityType.Document;
}
