namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class MemberConfigurationResponseModel
{
    [Obsolete("Use MemberTypeConfigurationResponseModel.ReservedFieldNames from the ConfigurationMemberTypeController endpoint instead.")]
    public required ISet<string> ReservedFieldNames { get; set; }
}
