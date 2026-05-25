using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the computed state of a user following an operation that may have changed it.
/// </summary>
public class UserStateResponseModel
{
    /// <summary>
    /// Gets or sets a reference to the user.
    /// </summary>
    public ReferenceByIdModel User { get; set; } = new();

    /// <summary>
    /// Gets or sets the current state of the user.
    /// </summary>
    public UserState State { get; set; }
}
