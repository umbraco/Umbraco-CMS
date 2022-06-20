namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides utilities to handle site domains.
/// </summary>
public interface ISiteDomainMapper
{
    /// <summary>
    ///     Filters a list of <c>DomainAndUri</c> to pick one that best matches the current request.
    /// </summary>
    /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
    /// <param name="current">The Uri of the current request.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="defaultCulture">The default culture.</param>
    /// <returns>The selected <c>DomainAndUri</c>.</returns>
    /// <remarks>
    ///     <para>
    ///         If the filter is invoked then <paramref name="domainAndUris" /> is _not_ empty and
    ///         <paramref name="current" /> is _not_ null, and <paramref name="current" /> could not be
    ///         matched with anything in <paramref name="domainAndUris" />.
    ///     </para>
    ///     <para>
    ///         The <paramref name="culture" /> may be null, but when non-null, it can be used
    ///         to help pick the best matches.
    ///     </para>
    ///     <para>The filter _must_ return something else an exception will be thrown.</para>
    /// </remarks>
    DomainAndUri? MapDomain(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, string? culture, string? defaultCulture);

    /// <summary>
    ///     Filters a list of <c>DomainAndUri</c> to pick those that best matches the current request.
    /// </summary>
    /// <param name="domainAndUris">The list of <c>DomainAndUri</c> to filter.</param>
    /// <param name="current">The Uri of the current request.</param>
    /// <param name="excludeDefault">A value indicating whether to exclude the current/default domain.</param>
    /// <param name="culture">A culture.</param>
    /// <param name="defaultCulture">The default culture.</param>
    /// <returns>The selected <c>DomainAndUri</c> items.</returns>
    /// <remarks>
    ///     <para>The filter must return something, even empty, else an exception will be thrown.</para>
    ///     <para>
    ///         The <paramref name="culture" /> may be null, but when non-null, it can be used
    ///         to help pick the best matches.
    ///     </para>
    /// </remarks>
    IEnumerable<DomainAndUri> MapDomains(IReadOnlyCollection<DomainAndUri> domainAndUris, Uri current, bool excludeDefault, string? culture, string? defaultCulture);
}
