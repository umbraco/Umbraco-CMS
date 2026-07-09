namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents the result of a member profile update operation.
/// </summary>
public class UpdateMemberProfileResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateMemberProfileResult" /> class.
    /// </summary>
    private UpdateMemberProfileResult()
    {
    }

    /// <summary>
    ///     Gets the status of the update operation.
    /// </summary>
    public UpdateMemberProfileStatus Status { get; private set; }

    /// <summary>
    ///     Gets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    /// <returns>A new <see cref="UpdateMemberProfileResult" /> indicating success.</returns>
    public static UpdateMemberProfileResult Success() =>
        new UpdateMemberProfileResult { Status = UpdateMemberProfileStatus.Success };

    /// <summary>
    ///     Creates an error result with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the failure.</param>
    /// <returns>A new <see cref="UpdateMemberProfileResult" /> indicating an error.</returns>
    public static UpdateMemberProfileResult Error(string message) =>
        new UpdateMemberProfileResult { Status = UpdateMemberProfileStatus.Error, ErrorMessage = message };
}
