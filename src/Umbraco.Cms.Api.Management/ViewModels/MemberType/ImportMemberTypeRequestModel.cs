namespace Umbraco.Cms.Api.Management.ViewModels.MemberType;

/// <summary>
/// Represents the data required to import a member type via an API request.
/// </summary>
public class ImportMemberTypeRequestModel
{
    /// <summary>
    /// Gets or sets a reference to the file from which to import the member type.
    /// </summary>
    public required ReferenceByIdModel File { get; set; }
}
