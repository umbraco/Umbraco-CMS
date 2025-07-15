using Examine;
using Umbraco.Cms.Core.Services;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Infrastructure.Examine;

public class UmbracoIndexConfig : IUmbracoIndexConfig
{
    public UmbracoIndexConfig(IPublicAccessService publicAccessService, IScopeProvider scopeProvider)
    {
        ScopeProvider = scopeProvider;
        PublicAccessService = publicAccessService;
    }

    protected IPublicAccessService PublicAccessService { get; }

    protected IScopeProvider ScopeProvider { get; }

    public IContentValueSetValidator GetContentValueSetValidator() =>
        new ContentValueSetValidator(false, true, PublicAccessService, ScopeProvider, null, null, null);

    public IContentValueSetValidator GetPublishedContentValueSetValidator() =>
        new ContentValueSetValidator(true, false, PublicAccessService, ScopeProvider, null, null, null);

    /// <summary>
    ///     Returns the <see cref="IValueSetValidator" /> for the member indexer
    /// </summary>
    /// <returns></returns>
    public IValueSetValidator GetMemberValueSetValidator() => new MemberValueSetValidator();
}
