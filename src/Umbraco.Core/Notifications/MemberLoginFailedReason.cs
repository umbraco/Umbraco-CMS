// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Well-known reason strings for <see cref="MemberLoginFailedNotification"/>.
/// </summary>
public static class MemberLoginFailedReason
{
    /// <summary>
    ///     The credentials provided were invalid (wrong password).
    /// </summary>
    public const string InvalidCredentials = "InvalidCredentials";

    /// <summary>
    ///     The member account is locked out due to too many failed attempts.
    /// </summary>
    public const string LockedOut = "LockedOut";

    /// <summary>
    ///     The member account is not allowed to sign in (e.g. not approved or email not confirmed).
    /// </summary>
    public const string NotAllowed = "NotAllowed";

    /// <summary>
    ///     No member was found for the given username.
    /// </summary>
    public const string MemberNotFound = "MemberNotFound";
}
