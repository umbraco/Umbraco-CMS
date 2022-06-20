// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Cms.Infrastructure.Examine;

public class UmbracoExamineIndexDiagnostics : LuceneIndexDiagnostics
{
    private readonly UmbracoExamineIndex _index;

    public UmbracoExamineIndexDiagnostics(
        UmbracoExamineIndex index,
        ILogger<UmbracoExamineIndexDiagnostics> logger,
        IHostingEnvironment hostingEnvironment,
        IOptionsMonitor<LuceneDirectoryIndexOptions> indexOptions)
        : base(index, logger, hostingEnvironment, indexOptions) =>
        _index = index;

    public override IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            var d = base.Metadata.ToDictionary(x => x.Key, x => x.Value);

            d[nameof(UmbracoExamineIndex.EnableDefaultEventHandler)] = _index.EnableDefaultEventHandler;
            d[nameof(UmbracoExamineIndex.PublishedValuesOnly)] = _index.PublishedValuesOnly;

            if (_index.ValueSetValidator is ValueSetValidator vsv)
            {
                d[nameof(ValueSetValidator.IncludeItemTypes)] = vsv.IncludeItemTypes;
                d[nameof(ContentValueSetValidator.ExcludeItemTypes)] = vsv.ExcludeItemTypes;
                d[nameof(ContentValueSetValidator.IncludeFields)] = vsv.IncludeFields;
                d[nameof(ContentValueSetValidator.ExcludeFields)] = vsv.ExcludeFields;
            }

            if (_index.ValueSetValidator is ContentValueSetValidator cvsv)
            {
                d[nameof(ContentValueSetValidator.PublishedValuesOnly)] = cvsv.PublishedValuesOnly;
                d[nameof(ContentValueSetValidator.SupportProtectedContent)] = cvsv.SupportProtectedContent;
                d[nameof(ContentValueSetValidator.ParentId)] = cvsv.ParentId;
            }

            return d.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
