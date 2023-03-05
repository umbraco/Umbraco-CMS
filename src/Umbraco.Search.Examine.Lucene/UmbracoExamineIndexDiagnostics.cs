// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine.Lucene;

public class UmbracoExamineIndexDiagnostics : LuceneIndexDiagnostics
{
    private readonly UmbracoExamineLuceneIndex _luceneIndex;

    public UmbracoExamineIndexDiagnostics(
        UmbracoExamineLuceneIndex luceneIndex,
        ILogger<UmbracoExamineIndexDiagnostics> logger,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        : base(luceneIndex, logger, hostingEnvironment, indexOptions) =>
        _luceneIndex = luceneIndex;

    public override IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            var d = base.Metadata.ToDictionary(x => x.Key, x => x.Value);

            d[nameof(UmbracoExamineLuceneIndex.EnableDefaultEventHandler)] = _luceneIndex.EnableDefaultEventHandler;
            d[nameof(UmbracoExamineLuceneIndex.PublishedValuesOnly)] = _luceneIndex.PublishedValuesOnly;

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
