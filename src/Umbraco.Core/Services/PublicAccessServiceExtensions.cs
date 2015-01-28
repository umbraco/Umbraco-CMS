using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Extension methods for the IPublicAccessService
    /// </summary>
    public static class PublicAccessServiceExtensions
    {

        internal static bool HasAccess(this IPublicAccessService publicAccessService, int documentId, IContentService contentService, IEnumerable<string> currentMemberRoles)
        {
            var content = contentService.GetById(documentId);
            if (content == null) return true;

            var entry = publicAccessService.GetEntryForContent(content);
            if (entry == null) return true;

            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType
                                        && currentMemberRoles.Contains(x.RuleValue));
        }

        [Obsolete("this is only used for backward compat")]
        internal static bool HasAccess(this IPublicAccessService publicAccessService, int documentId, object providerUserKey, IContentService contentService, MembershipProvider membershipProvider, RoleProvider roleProvider)
        {
            var content = contentService.GetById(documentId);
            if (content == null) return true;

            var entry = publicAccessService.GetEntryForContent(content);
            if (entry == null) return true;

            var member = membershipProvider.GetUser(providerUserKey, false);
            if (member == null) return false;

            var roles = roleProvider.GetRolesForUser(member.UserName);
            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType
                                        && roles.Contains(x.RuleValue));
        }

        public static bool HasAccess(this IPublicAccessService publicAccessService, string path, MembershipUser member, RoleProvider roleProvider)
        {
            var entry = publicAccessService.GetEntryForContent(path.EnsureEndsWith(path));
            if (entry == null) return true;

            var roles = roleProvider.GetRolesForUser(member.UserName);
            return entry.Rules.Any(x => x.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType
                                        && roles.Contains(x.RuleValue));
        }
    }
}