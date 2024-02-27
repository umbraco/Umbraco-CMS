namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class MemberConfigurationResponseModel
{
    public required bool DisableDeleteWhenReferenced { get; set; }

    public required bool DisableUnpublishWhenReferenced { get; set; }

    public required bool SanitizeTinyMce { get; set; }
    public required ISet<string> StandardFieldNames { get; set; }
}
