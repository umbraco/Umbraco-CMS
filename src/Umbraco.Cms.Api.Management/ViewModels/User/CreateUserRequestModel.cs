using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model containing the information required to create a new user.
/// </summary>
public class CreateUserRequestModel : CreateUserRequestModelBase
{
    /// <summary>
    /// Gets or sets the type or category of the user being created.
    /// </summary>
    public UserKind Kind { get; set; }
}
