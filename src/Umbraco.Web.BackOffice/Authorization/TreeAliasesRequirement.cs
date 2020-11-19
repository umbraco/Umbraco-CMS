using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Umbraco.Web.BackOffice.Authorization
{

    /// <summary>
    /// Authorization requirements for <see cref="UmbracoTreeAuthorizeHandler"/>
    /// </summary>
    public class TreeAliasesRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The aliases for trees that the user will need access to
        /// </summary>
        public IReadOnlyCollection<string> TreeAliases { get; }

        public TreeAliasesRequirement(params string[] aliases) => TreeAliases = aliases;
    }
}
