using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public class DocumentRecycleBinItemResponseModel : RecycleBinItemResponseModel
{
    public override string Type => Constants.UdiEntityType.DocumentInRecycleBin;
}

