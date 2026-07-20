namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Represents a response model containing configuration details for a member type.
/// </summary>
public class MemberTypeConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets the collection of field names that are reserved and cannot be used in the member type configuration.
    /// </summary>
    public required ISet<string> ReservedFieldNames { get; set; }
}
