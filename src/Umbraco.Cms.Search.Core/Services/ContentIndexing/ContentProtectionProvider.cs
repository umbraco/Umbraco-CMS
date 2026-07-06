using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

internal sealed class ContentProtectionProvider : IContentProtectionProvider
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IMemberService _memberService;
    private readonly IMemberGroupService _memberGroupService;

    public ContentProtectionProvider(IPublicAccessService publicAccessService, IMemberService memberService, IMemberGroupService memberGroupService)
    {
        _publicAccessService = publicAccessService;
        _memberService = memberService;
        _memberGroupService = memberGroupService;
    }

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
