namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents filter criteria for querying members.
/// </summary>
public class MemberFilter
{
    /// <summary>
    ///     Gets or sets the member type identifier to filter by.
    /// </summary>
    public Guid? MemberTypeId { get; set; }

    /// <summary>
    ///     Gets or sets the member group name to filter by.
    /// </summary>
    public string? MemberGroupName { get; set; }

    /// <summary>
    ///     Gets or sets a value to filter by approval status.
    /// </summary>
    public bool? IsApproved { get; set; }

    /// <summary>
    ///     Gets or sets a value to filter by locked out status.
    /// </summary>
    public bool? IsLockedOut { get; set; }

    /// <summary>
    ///     Gets or sets a text filter to search members by name, email, or username.
    /// </summary>
    public string? Filter { get; set; }
}
