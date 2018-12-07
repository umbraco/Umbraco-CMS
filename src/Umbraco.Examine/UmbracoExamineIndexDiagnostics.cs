using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.IO;
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

        public virtual IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var d = new Dictionary<string, object>
                {
                    [nameof(UmbracoExamineIndexer.CommitCount)] = _index.CommitCount,
                    [nameof(UmbracoExamineIndexer.DefaultAnalyzer)] = _index.DefaultAnalyzer.GetType().Name,
                    [nameof(UmbracoExamineIndexer.DirectoryFactory)] = _index.DirectoryFactory,
                    [nameof(UmbracoExamineIndexer.EnableDefaultEventHandler)] = _index.EnableDefaultEventHandler,
                    [nameof(UmbracoExamineIndexer.LuceneIndexFolder)] =
                        _index.LuceneIndexFolder == null
                            ? string.Empty
                            : _index.LuceneIndexFolder.ToString().ToLowerInvariant().TrimStart(IOHelper.MapPath(SystemDirectories.Root).ToLowerInvariant()).Replace("\\", "/").EnsureStartsWith('/'),
                    [nameof(UmbracoExamineIndexer.PublishedValuesOnly)] = _index.PublishedValuesOnly,
                    //There's too much info here
                    //[nameof(UmbracoExamineIndexer.FieldDefinitionCollection)] = _index.FieldDefinitionCollection,
                };

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
}
