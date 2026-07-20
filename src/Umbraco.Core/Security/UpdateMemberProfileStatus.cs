namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents the status of a member profile update operation.
/// </summary>
public enum UpdateMemberProfileStatus
{
    /// <summary>
    ///     The profile update was successful.
    /// </summary>
    Success,

    /// <summary>
    ///     The profile update failed with an error.
    /// </summary>
    Error,
}
