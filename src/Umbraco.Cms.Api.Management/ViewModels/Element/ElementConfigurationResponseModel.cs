namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ElementConfigurationResponseModel
{
    public required bool DisableDeleteWhenReferenced { get; set; }

    public required bool DisableUnpublishWhenReferenced { get; set; }

    public required bool AllowEditInvariantFromNonDefault { get; set; }

    [Obsolete("This functionality will be moved to a client-side extension. Scheduled for removal in V19.")]
    public required bool AllowNonExistingSegmentsCreation { get; set; }
}
