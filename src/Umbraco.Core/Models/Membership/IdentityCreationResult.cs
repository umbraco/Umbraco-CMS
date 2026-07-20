namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the result of an identity creation operation.
/// </summary>
public class IdentityCreationResult
{
    /// <summary>
    ///     Creates a failed identity creation result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <returns>A failed <see cref="IdentityCreationResult" /> instance.</returns>
    public static IdentityCreationResult Fail(string errorMessage) =>
        new()
        {
            ErrorMessage = errorMessage,
            Succeded = false,
        };

    /// <summary>
    ///     Creates a failed identity creation result indicating the operation was cancelled by a notification handler.
    /// </summary>
    /// <returns>A failed <see cref="IdentityCreationResult" /> instance with <see cref="CancelledByNotification" /> set.</returns>
    public static IdentityCreationResult Cancel() =>
        new()
        {
            Succeded = false,
            CancelledByNotification = true,
        };

    /// <summary>
    ///     Gets or initializes a value indicating whether the identity creation succeeded.
    /// </summary>
    public bool Succeded { get; init; }

    /// <summary>
    ///     Gets or initializes a value indicating whether the operation was cancelled by a notification handler.
    /// </summary>
    public bool CancelledByNotification { get; init; }

    /// <summary>
    ///     Gets or initializes the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///     Gets or initializes the initial password generated for the new identity.
    /// </summary>
    public string? InitialPassword { get; init; }
}
