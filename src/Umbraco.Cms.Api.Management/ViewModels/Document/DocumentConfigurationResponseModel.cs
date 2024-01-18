namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentConfigurationResponseModel
{
    public bool SanitizeTinyMce { get; set; }

    public bool DisableDeleteWhenReferenced { get; set; }

    public bool DisableUnpublishWhenReferenced { get; set; }

    public bool AllowEditInvariantFromNonDefault { get; set; }
}
