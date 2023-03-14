using Examine;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

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
        new ContentValueSetValidator(false, true, PublicAccessService, ScopeProvider);

    public IContentValueSetValidator GetPublishedContentValueSetValidator() =>
        new ContentValueSetValidator(true, false, PublicAccessService, ScopeProvider);

    /// <summary>
    ///     Returns the <see cref="IValueSetValidator" /> for the member indexer
    /// </summary>
    /// <returns></returns>
    public IValueSetValidator GetMemberValueSetValidator() => new MemberValueSetValidator();
}
