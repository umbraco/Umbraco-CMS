using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

public class MediaRecycleBinItemResponseModel : RecycleBinItemResponseModel
{
    public override string Type => Constants.UdiEntityType.MediaInRecycleBin;
}

