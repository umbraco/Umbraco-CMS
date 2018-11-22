using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A resolver to map the provider field aliases
    /// </summary>
    internal class MemberProviderFieldResolver
    {
        public IDictionary<string, string> Resolve(IMember source)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider() == false)
            {
                return new Dictionary<string, string>
                {
                    {Constants.Conventions.Member.IsLockedOut, Constants.Conventions.Member.IsLockedOut},
                    {Constants.Conventions.Member.IsApproved, Constants.Conventions.Member.IsApproved},
                    {Constants.Conventions.Member.Comments, Constants.Conventions.Member.Comments}
                };
            }
            else
            {
                var umbracoProvider = (IUmbracoMemberTypeMembershipProvider) provider;

                return new Dictionary<string, string>
                {
                    {Constants.Conventions.Member.IsLockedOut, umbracoProvider.LockPropertyTypeAlias},
                    {Constants.Conventions.Member.IsApproved, umbracoProvider.ApprovedPropertyTypeAlias},
                    {Constants.Conventions.Member.Comments, umbracoProvider.CommentPropertyTypeAlias}
                };
            }
        }
    }
}
