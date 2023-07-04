using Examine.Lucene;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.Configuration;

namespace Umbraco.Search.Examine.Lucene;

/// <summary>
///     Creates <see cref="IIndexDiagnostics" /> for an index if it doesn't implement <see cref="IIndexDiagnostics" />
/// </summary>
public class ExamineLuceneIndexDiagnosticsFactory : IIndexDiagnosticsFactory
{
    private readonly ISearchProvider _provider;
    private readonly IUmbracoIndexesConfiguration _configuration;
    private readonly ILogger<UmbracoExamineIndexDiagnostics> _logger;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IOptionsMonitor<LuceneDirectoryIndexOptions> _options;

    public ExamineLuceneIndexDiagnosticsFactory(ISearchProvider provider,
        IUmbracoIndexesConfiguration configuration, ILogger<UmbracoExamineIndexDiagnostics> logger, IHostingEnvironment hostingEnvironment, IOptionsMonitor<LuceneDirectoryIndexOptions> options)
    {
        _provider = provider;
        _configuration = configuration;
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
        return new UmbracoExamineIndexDiagnostics(indexTarget.ExamineIndex,_configuration,_logger,_hostingEnvironment,_options);

    }

}
