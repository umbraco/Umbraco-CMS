namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaConfigurationResponseModel
{
    public bool DisableDeleteWhenReferenced { get; set; }

    public bool DisableUnpublishWhenReferenced { get; set; }

    public bool SanitizeTinyMce { get; set; }
}
