using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.IO;

namespace Umbraco.Examine
{
    /// <summary>
    /// Implementation of <see cref="IIndexDiagnosticsFactory"/> which returns <see cref="LuceneIndexDiagnostics"/>
    /// for lucene based indexes that don't have an implementation else fallsback to the default <see cref="IndexDiagnosticsFactory"/> implementation.
    /// </summary>
    public class LuceneIndexDiagnosticsFactory : IndexDiagnosticsFactory
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public LuceneIndexDiagnosticsFactory(ILogger logger, IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        public override IIndexDiagnostics Create(IIndex index)
        {
            if (!(index is IIndexDiagnostics indexDiag))
            {
                if (index is LuceneIndex luceneIndex)
                    indexDiag = new LuceneIndexDiagnostics(luceneIndex, _logger, _hostingEnvironment);
                else
                    indexDiag = base.Create(index);
            }
            return indexDiag;
        }
    }
}
