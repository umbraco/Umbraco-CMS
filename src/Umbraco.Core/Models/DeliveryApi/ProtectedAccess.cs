namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents protected access information for content in the Delivery API.
/// </summary>
public sealed class ProtectedAccess
{
    /// <summary>
    ///     Gets a <see cref="ProtectedAccess" /> instance representing no protection.
    /// </summary>
    public static ProtectedAccess None => new(null, null);

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProtectedAccess" /> class.
    /// </summary>
    /// <param name="memberKey">The unique identifier of the member who has access.</param>
    /// <param name="memberRoles">The member roles that have access.</param>
    public ProtectedAccess(Guid? memberKey, string[]? memberRoles)
    {
        MemberKey = memberKey;
        MemberRoles = memberRoles;
    }

    /// <summary>
    ///     Gets the unique identifier of the member who has access.
    /// </summary>
    public Guid? MemberKey { get; }

    /// <summary>
    ///     Gets the member roles that have access.
    /// </summary>
    public string[]? MemberRoles { get; }
}
