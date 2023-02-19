// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Search.Examine;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An indexer for Umbraco content and media
/// </summary>
public class UmbracoContentIndex : UmbracoExamineIndex<IContent>
{
    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    public UmbracoContentIndex(
        ILoggerFactory loggerFactory,
        IIndex index, IValueSetBuilder<IContent> valueSetBuilder)
        : base(index,valueSetBuilder)
    {

    }

    protected ILocalizationService? LanguageService { get; }

}
