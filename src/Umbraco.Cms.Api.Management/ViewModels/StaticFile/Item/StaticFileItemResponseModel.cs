using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;

public class StaticFileItemResponseModel : FileItemResponseModelBase
{
    public override string Type => Constants.UdiEntityType.StaticFile;
}
