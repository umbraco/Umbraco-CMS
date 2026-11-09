using Umbraco.Cms.Core.Search.Indexing;
using MemberFieldNames = Umbraco.Cms.Core.Constants.Search.MemberFieldNames;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;

/// <summary>
/// Builds the member system fields shared by content-based members and external members.
/// </summary>
internal static class MemberIndexFields
{
    /// <summary>
    /// Creates the member system fields.
    /// </summary>
    /// <param name="email">The member's email address.</param>
    /// <param name="userName">The member's username.</param>
    /// <param name="isApproved">Whether the member is approved.</param>
    /// <param name="isLockedOut">Whether the member is locked out.</param>
    /// <param name="isExternalMember">Whether the member is an external member rather than a content-based member.</param>
    /// <returns>The member system fields.</returns>
    public static IEnumerable<IndexField> Create(string email, string userName, bool isApproved, bool isLockedOut, bool isExternalMember)
        =>
        [
            new(MemberFieldNames.Email, new() { TextsR2 = [email], Keywords = [email] }, null, null),
            new(MemberFieldNames.UserName, new() { TextsR2 = [userName], Keywords = [userName] }, null, null),
            new(MemberFieldNames.IsApproved, new() { Integers = [isApproved ? 1 : 0] }, null, null),
            new(MemberFieldNames.IsLockedOut, new() { Integers = [isLockedOut ? 1 : 0] }, null, null),
            new(MemberFieldNames.IsExternalMember, new() { Integers = [isExternalMember ? 1 : 0] }, null, null),
        ];
}
