using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing.Indexers;

/// <summary>
/// Indexes the member system fields (email, username, approval and lockout state) for content-based members.
/// </summary>
internal sealed class MemberFieldsContentIndexer : IContentIndexer
{
    /// <inheritdoc />
    public Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken)
        => Task.FromResult(content is IMember member
            ? MemberIndexFields.Create(member.Email, member.Username, member.IsApproved, member.IsLockedOut, isExternalMember: false)
            : Enumerable.Empty<IndexField>());
}
