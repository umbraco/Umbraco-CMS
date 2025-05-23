using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache;

// note
// the whole PublishedMember thing should be refactored because as soon as a member
// is wrapped on in a model, the inner IMember and all associated properties are lost
internal class PublishedMember : PublishedContent, IPublishedMember
{
    private readonly IMember _member;

    public PublishedMember(
        IMember member,
        ContentNode contentNode,
        IElementsCache elementsCache,
        IVariationContextAccessor variationContextAccessor)
        : base(contentNode, false, elementsCache, variationContextAccessor) =>
        _member = member;

    public string Email => _member.Email;

    public string UserName => _member.Username;

    public string? Comments => _member.Comments;

    public bool IsApproved => _member.IsApproved;

    public bool IsLockedOut => _member.IsLockedOut;

    public DateTime? LastLockoutDate => _member.LastLockoutDate;

    public DateTime CreationDate => _member.CreateDate;

    public DateTime? LastLoginDate => _member.LastLoginDate;

    public DateTime? LastPasswordChangedDate => _member.LastPasswordChangeDate;
}
