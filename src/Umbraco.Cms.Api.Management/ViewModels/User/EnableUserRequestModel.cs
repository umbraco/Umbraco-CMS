namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Request model for enabling a user.
/// </summary>
public class EnableUserRequestModel
{
    /// <summary>
    /// Gets or sets the set of user references (by ID) to enable.
    /// </summary>
    public ISet<ReferenceByIdModel> UserIds { get; set; } = new HashSet<ReferenceByIdModel>();
}
