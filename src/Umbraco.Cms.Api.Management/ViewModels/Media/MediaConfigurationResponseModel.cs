namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaConfigurationResponseModel
{
    public required bool DisableDeleteWhenReferenced { get; set; }

    [Obsolete("Media cannot be published or unpublished, so this property is not applicable. Scheduled for removal in Umbraco 19.")]
    public required bool DisableUnpublishWhenReferenced { get; set; }
}
