using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Default implementation of <see cref="IContentProtectionProvider"/>, based on <see cref="IPublicAccessService"/> entries.
/// </summary>
internal sealed class ContentProtectionProvider : IContentProtectionProvider
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberService _memberService;
    private readonly IMemberGroupService _memberGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentProtectionProvider"/> class.
    /// </summary>
    /// <param name="publicAccessService">The service used to look up the public access entry for a content item's path.</param>
    /// <param name="memberService">The service used to resolve member keys from usernames.</param>
    /// <param name="memberGroupService">The service used to resolve member group keys from role names.</param>
    public ContentProtectionProvider(IPublicAccessService publicAccessService, IMemberService memberService, IMemberGroupService memberGroupService)
    {
        _publicAccessService = publicAccessService;
        _memberService = memberService;
        _memberGroupService = memberGroupService;
    }

    /// <inheritdoc />
    public async Task<ContentProtection?> GetContentProtectionAsync(IContentBase content)
    {
        if (content is not IContent)
        {
            return null;
        }

        PublicAccessEntry? publicAccessEntry = _publicAccessService.GetEntryForContent(content.Path);
        if (publicAccessEntry is null)
        {
            return null;
        }

        var roles = RuleValues(publicAccessEntry, Umbraco.Cms.Core.Constants.Conventions.PublicAccess.MemberRoleRuleType);
        var usernames = RuleValues(publicAccessEntry, Umbraco.Cms.Core.Constants.Conventions.PublicAccess.MemberUsernameRuleType);

        var accessKeys = new List<Guid>();

        if (roles.Length > 0)
        {
            IEnumerable<IMemberGroup> memberGroups = await _memberGroupService.GetAllAsync();
            accessKeys.AddRange(
                memberGroups
                    .Where(role => role.Name.IsNullOrWhiteSpace() is false && roles.InvariantContains(role.Name))
                    .Select(role => role.Key));
        }

        if (usernames.Length > 0)
        {
            accessKeys.AddRange(
                usernames.Select(username => _memberService.GetByUsername(username)?.Key ?? null)
                    .Where(key => key.HasValue)
                    .Select(key => key!.Value));
        }

        return accessKeys.Count > 0 ? new ContentProtection(accessKeys) : null;

        string[] RuleValues(PublicAccessEntry entry, string ruleType)
            => entry.Rules
                .Where(rule => rule.RuleType == ruleType && rule.RuleValue.IsNullOrWhiteSpace() is false)
                .Select(rule => rule.RuleValue!)
                .ToArray();
    }
}
