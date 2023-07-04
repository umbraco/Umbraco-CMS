// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Search.Configuration;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine.Lucene;

public class UmbracoExamineIndexDiagnostics : LuceneIndexDiagnostics
{
    private readonly UmbracoExamineLuceneIndex _luceneIndex;
    private readonly IUmbracoIndexesConfiguration _configuration;

    public UmbracoExamineIndexDiagnostics(
        UmbracoExamineLuceneIndex luceneIndex,
        IUmbracoIndexesConfiguration configuration,
        ILogger<UmbracoExamineIndexDiagnostics> logger,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        : base(luceneIndex, logger, hostingEnvironment, indexOptions)
    {
        _luceneIndex = luceneIndex;
        _configuration = configuration;
    }

    public override IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            var d = base.Metadata.ToDictionary(x => x.Key, x => x.Value);
            var configuration = _configuration.Configuration(_luceneIndex.Name);
            d[nameof(configuration.EnableDefaultEventHandler)] = configuration.EnableDefaultEventHandler;
            d[nameof(configuration.PublishedValuesOnly)] = configuration.PublishedValuesOnly;

            if (_luceneIndex.ValueSetValidator is ValueSetValidator vsv)
            {
                d[nameof(ValueSetValidator.IncludeItemTypes)] = vsv.IncludeItemTypes;
                d[nameof(ContentValueSetValidator.ExcludeItemTypes)] = vsv.ExcludeItemTypes;
                d[nameof(ContentValueSetValidator.IncludeFields)] = vsv.IncludeFields;
                d[nameof(ContentValueSetValidator.ExcludeFields)] = vsv.ExcludeFields;
            }

            if (_luceneIndex.ValueSetValidator is ContentValueSetValidator cvsv)
            {
                d[nameof(ContentValueSetValidator.PublishedValuesOnly)] = cvsv.PublishedValuesOnly;
                d[nameof(ContentValueSetValidator.SupportProtectedContent)] = cvsv.SupportProtectedContent;
                d[nameof(ContentValueSetValidator.ParentId)] = cvsv.ParentId;
            }

            return d.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
