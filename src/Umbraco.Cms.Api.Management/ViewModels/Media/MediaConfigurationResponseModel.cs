namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public class MediaConfigurationResponseModel
{
    public required bool DisableDeleteWhenReferenced { get; set; }

    public required bool DisableUnpublishWhenReferenced { get; set; }

    [Obsolete("Use MediaTypeConfigurationResponseModel.ReservedFieldNames from the MediaTypeConfiguration endpoint instead.")]
    public required ISet<string> ReservedFieldNames { get; set; }
}
