namespace Umbraco.Cms.Api.Management.ViewModels.Member;

public class MemberConfigurationResponseModel
{
    [Obsolete("Use MemberTypeConfigurationResponseModel.ReservedFieldNames from the MemberTypeConfiguration endpoint instead.")]
    public required ISet<string> ReservedFieldNames { get; set; }
}
