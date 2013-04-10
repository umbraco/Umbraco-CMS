using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides utilities to handle site domains.
    /// </summary>
    public interface ISiteDomainHelper
    {
        /// <summary>
        /// Filters a list of <c>DomainAndUri</c> to pick one that best matches the current request.
        /// </summary>
        /// <param name="current">The Uri of the current request.</param>
        /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
        /// <returns>The selected <c>DomainAndUri</c>.</returns>
        /// <remarks>
        /// <para>If the filter is invoked then <paramref name="domainAndUris"/> is _not_ empty and
        /// <paramref name="current"/> is _not_ null, and <paramref name="current"/> could not be
        /// matched with anything in <paramref name="domainAndUris"/>.</para>
        /// <para>The filter _must_ return something else an exception will be thrown.</para>
        /// </remarks>
        DomainAndUri MapDomain(Uri current, DomainAndUri[] domainAndUris);

        /// <summary>
        /// Filters a list of <c>DomainAndUri</c> to pick those that best matches the current request.
        /// </summary>
        /// <param name="current">The Uri of the current request.</param>
        /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
        /// <param name="excludeDefault">A value indicating whether to exclude the current/default domain.</param>
        /// <returns>The selected <c>DomainAndUri</c> items.</returns>
        /// <remarks>The filter must return something, even empty, else an exception will be thrown.</remarks>
        IEnumerable<DomainAndUri> MapDomains(Uri current, DomainAndUri[] domainAndUris, bool excludeDefault);
    }
}
