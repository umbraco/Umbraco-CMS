using Examine;
using Examine.Lucene.Analyzers;
using Lucene.Net.Analysis;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public class UmbracoIndexConfig : IUmbracoExamineIndexConfig
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

    public bool PublishedValuesOnly { get; set; }
    public bool EnableDefaultEventHandler { get; set; } = true;
    public Analyzer Analyzer { get; set; }  = new CultureInvariantWhitespaceAnalyzer();
}
