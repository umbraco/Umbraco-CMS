using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorization requirements for <see cref="UmbracoSectionAuthorizeHandler"/>
    /// </summary>
    public class SectionAliasesRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The aliases for sections that the user will need access to
        /// </summary>
        public IReadOnlyCollection<string> SectionAliases { get; }

        public SectionAliasesRequirement(params string[] aliases) => SectionAliases = aliases;
    }
}
