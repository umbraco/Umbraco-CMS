// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for the IPublicAccessService
/// </summary>
public static class PublicAccessServiceExtensions
{
    public static bool RenameMemberGroupRoleRules(this IPublicAccessService publicAccessService, string? oldRolename, string? newRolename)
    {
        var hasChange = false;
        if (oldRolename == newRolename)
        {
            return false;
        }

        IEnumerable<PublicAccessEntry> allEntries = publicAccessService.GetAll();

        foreach (PublicAccessEntry entry in allEntries)
        {
            // get rules that match
            IEnumerable<PublicAccessRule> roleRules = entry.Rules
                .Where(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
                .Where(x => x.RuleValue == oldRolename);
            var save = false;
            foreach (PublicAccessRule roleRule in roleRules)
            {
                // a rule is being updated so flag this entry to be saved
                roleRule.RuleValue = newRolename ?? string.Empty;
                save = true;
            }

            if (save)
            {
                hasChange = true;
                publicAccessService.Save(entry);
            }
        }

        return hasChange;
    }

    public static bool HasAccess(this IPublicAccessService publicAccessService, int documentId, IContentService contentService, string username, IEnumerable<string> currentMemberRoles)
    {
        IContent? content = contentService.GetById(documentId);
        if (content == null)
        {
            return true;
        }

        PublicAccessEntry? entry = publicAccessService.GetEntryForContent(content);
        if (entry == null)
        {
            return true;
        }

        return HasAccess(entry, username, currentMemberRoles);
    }

    /// <summary>
    ///     Checks if the member with the specified username has access to the path which is also based on the passed in roles
    ///     for the member
    /// </summary>
    /// <param name="publicAccessService"></param>
    /// <param name="path"></param>
    /// <param name="username"></param>
    /// <param name="rolesCallback">A callback to retrieve the roles for this member</param>
    /// <returns></returns>
    public static async Task<bool> HasAccessAsync(this IPublicAccessService publicAccessService, string path, string username, Func<Task<IEnumerable<string>>> rolesCallback)
    {
        if (rolesCallback == null)
        {
            throw new ArgumentNullException("roles");
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "username");
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", "path");
        }

        PublicAccessEntry? entry = publicAccessService.GetEntryForContent(path.EnsureEndsWith(path));
        if (entry == null)
        {
            return true;
        }

        IEnumerable<string> roles = await rolesCallback();

        return HasAccess(entry, username, roles);
    }

    private static bool HasAccess(PublicAccessEntry entry, string username, IEnumerable<string> roles)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException($"'{nameof(username)}' cannot be null or empty.", nameof(username));
        }

        if (roles is null)
        {
            throw new ArgumentNullException(nameof(roles));
        }

        return entry.Rules.Any(x =>
            (x.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType &&
             username.Equals(x.RuleValue, StringComparison.OrdinalIgnoreCase))
            || (x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType && roles.Contains(x.RuleValue)));
    }
}
