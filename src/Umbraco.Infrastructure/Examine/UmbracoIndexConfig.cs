using Examine;
using Umbraco.Cms.Core.Services;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents the configuration settings for an Umbraco Examine index.
/// </summary>
public class UmbracoIndexConfig : IUmbracoIndexConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoIndexConfig"/> class with the specified public access service and scope provider.
    /// </summary>
    /// <param name="publicAccessService">Service used to manage public access restrictions for content.</param>
    /// <param name="scopeProvider">Provider for managing database transaction scopes.</param>
    public UmbracoIndexConfig(IPublicAccessService publicAccessService, IScopeProvider scopeProvider)
    {
        ScopeProvider = scopeProvider;
        PublicAccessService = publicAccessService;
    }

    protected IPublicAccessService PublicAccessService { get; }

    protected IScopeProvider ScopeProvider { get; }

    /// <summary>
    /// Gets a content value set validator configured for use with content indexing.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IContentValueSetValidator"/> initialized with the current configuration and services.
    /// </returns>
    public IContentValueSetValidator GetContentValueSetValidator() =>
        new ContentValueSetValidator(false, true, PublicAccessService, ScopeProvider, null, null, null);

    /// <summary>
    /// Gets the <see cref="IContentValueSetValidator"/> instance used to validate published content value sets.
    /// </summary>
    /// <returns>An <see cref="IContentValueSetValidator"/> configured for published content.</returns>
    public IContentValueSetValidator GetPublishedContentValueSetValidator() =>
        new ContentValueSetValidator(true, false, PublicAccessService, ScopeProvider, null, null, null);

    /// <summary>
    ///     Returns the <see cref="IValueSetValidator" /> for the member indexer
    /// </summary>
    /// <returns></returns>
    public IValueSetValidator GetMemberValueSetValidator() => new MemberValueSetValidator();
}
