namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the state of a consent.
/// </summary>
[Flags]
public enum ConsentState // : int
{
    // note - this is a [Flags] enumeration
    // on can create detailed flags such as:
    // GrantedOptIn = Granted | 0x0001
    // GrandedByForce = Granted | 0x0002
    //
    // 16 situations for each Pending/Granted/Revoked should be ok

    /// <summary>
    ///     There is no consent.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Consent is pending and has not been granted yet.
    /// </summary>
    Pending = 0x10000,

    /// <summary>
    ///     Consent has been granted.
    /// </summary>
    Granted = 0x20000,

    /// <summary>
    ///     Consent has been revoked.
    /// </summary>
    Revoked = 0x40000,
}
