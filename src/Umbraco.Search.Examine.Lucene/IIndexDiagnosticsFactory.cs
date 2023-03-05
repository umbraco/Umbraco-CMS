using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Search.Examine;

namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public class ExamineLuceneIndexDiagnosticsFactory : IIndexDiagnosticsFactory
{
    private readonly ISearchProvider _provider;
    private readonly ILogger<UmbracoExamineIndexDiagnostics> _logger;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IOptionsMonitor<LuceneDirectoryIndexOptions> _options;

    public ExamineLuceneIndexDiagnosticsFactory(ISearchProvider provider, ILogger<UmbracoExamineIndexDiagnostics> logger, IHostingEnvironment hostingEnvironment, IOptionsMonitor<LuceneDirectoryIndexOptions> options)
    {
        _provider = provider;
        _logger = logger;
        _hostingEnvironment = hostingEnvironment;
        _options = options;
    }

    public virtual IIndexDiagnostics Create(string index)
    {
        var indexTarget = _provider.GetIndex(index) as UmbracoExamineIndex;
        if (indexTarget == null)
        {
            return new GenericIndexDiagnostics(_provider.GetIndex(index), _provider.GetSearcher(index));
        }
        return new UmbracoExamineIndexDiagnostics(indexTarget.ExamineIndex,_logger,_hostingEnvironment,_options);

    }

}
