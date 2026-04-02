namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the result of a user creation operation.
/// </summary>
public class UserCreationResult : ErrorMessageResult
{
    /// <summary>
    ///     Gets or initializes the newly created user, if the operation succeeded.
    /// </summary>
    public IUser? CreatedUser { get; init; }

    /// <summary>
    ///     Gets or initializes the initial password generated for the new user.
    /// </summary>
    public string? InitialPassword { get; init; }
}
