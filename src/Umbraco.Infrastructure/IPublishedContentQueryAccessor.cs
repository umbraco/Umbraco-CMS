using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core;

/// <remarks>
///     Not intended for use in background threads where you should make use of
///     <see cref="Umbraco.Cms.Core.Web.IUmbracoContextFactory.EnsureUmbracoContext" />
///     and instead resolve IPublishedContentQuery from a
///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceScope" />
///     e.g. using <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.CreateScope" />
///     <example>
///         <code>
/// // Background thread example
/// using UmbracoContextReference _ = _umbracoContextFactory.EnsureUmbracoContext();
/// using IServiceScope serviceScope = _serviceProvider.CreateScope();
/// IPublishedContentQuery query = serviceScope.ServiceProvider.GetRequiredService&lt;IPublishedContentQuery&gt;();
/// </code>
///     </example>
/// </remarks>
public interface IPublishedContentQueryAccessor
{
    bool TryGetValue([MaybeNullWhen(false)] out IPublishedContentQuery publishedContentQuery);
}
