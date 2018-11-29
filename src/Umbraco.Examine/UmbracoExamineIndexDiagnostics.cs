using System.Collections.Generic;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Examine
{
    public class UmbracoExamineIndexDiagnostics : IIndexDiagnostics
    {
        private readonly UmbracoExamineIndexer _index;
        private readonly ILogger _logger;

        public UmbracoExamineIndexDiagnostics(UmbracoExamineIndexer index, ILogger logger)
        {
            _index = index;
            _logger = logger;
        }

        public int DocumentCount
        {
            get
            {
                try
                {
                    return _index.GetIndexDocumentCount();
                }
                catch (AlreadyClosedException)
                {
                    _logger.Warn(typeof(UmbracoContentIndexer), "Cannot get GetIndexDocumentCount, the writer is already closed");
                    return 0;
                }
            }
        }

        public int FieldCount
        {
            get
            {
                try
                {
                    return _index.GetIndexFieldCount();
                }
                catch (AlreadyClosedException)
                {
                    _logger.Warn(typeof(UmbracoContentIndexer), "Cannot get GetIndexFieldCount, the writer is already closed");
                    return 0;
                }
            }
        }

        public Attempt<string> IsHealthy()
        {
            var isHealthy = _index.IsHealthy(out var indexError);
            return isHealthy ? Attempt<string>.Succeed() : Attempt.Fail(indexError.Message);
        }

        public virtual IReadOnlyDictionary<string, object> Metadata => new Dictionary<string, object>
        {
            [nameof(UmbracoExamineIndexer.CommitCount)] = _index.CommitCount,
            [nameof(UmbracoExamineIndexer.DefaultAnalyzer)] = _index.DefaultAnalyzer.GetType(),
            [nameof(UmbracoExamineIndexer.DirectoryFactory)] = _index.DirectoryFactory,
            [nameof(UmbracoExamineIndexer.EnableDefaultEventHandler)] = _index.EnableDefaultEventHandler,
            [nameof(UmbracoExamineIndexer.LuceneIndexFolder)] = _index.LuceneIndexFolder?.ToString(),
            [nameof(UmbracoExamineIndexer.SupportSoftDelete)] = _index.SupportSoftDelete,
            [nameof(UmbracoExamineIndexer.FieldDefinitionCollection)] = _index.FieldDefinitionCollection,
            [nameof(UmbracoExamineIndexer.FieldValueTypeCollection)] = _index.FieldValueTypeCollection,
            
        };
    }
}
