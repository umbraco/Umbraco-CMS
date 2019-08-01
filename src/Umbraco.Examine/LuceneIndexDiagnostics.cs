using System.Collections.Generic;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Lucene.Net.Store;
using Umbraco.Core.IO;

namespace Umbraco.Examine
{
    public class LuceneIndexDiagnostics : IIndexDiagnostics
    {
        public LuceneIndexDiagnostics(LuceneIndex index, ILogger logger)
        {
            Index = index;
            Logger = logger;
        }

        public LuceneIndex Index { get; }
        public ILogger Logger { get; }

        public int DocumentCount
        {
            get
            {
                try
                {
                    return Index.GetIndexDocumentCount();
                }
                catch (AlreadyClosedException)
                {
                    Logger.Warn(typeof(UmbracoContentIndex), "Cannot get GetIndexDocumentCount, the writer is already closed");
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
                    return Index.GetIndexFieldCount();
                }
                catch (AlreadyClosedException)
                {
                    Logger.Warn(typeof(UmbracoContentIndex), "Cannot get GetIndexFieldCount, the writer is already closed");
                    return 0;
                }
            }
        }

        public Attempt<string> IsHealthy()
        {
            var isHealthy = Index.IsHealthy(out var indexError);
            return isHealthy ? Attempt<string>.Succeed() : Attempt.Fail(indexError.Message);
        }

        public virtual IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var d = new Dictionary<string, object>
                {
                    [nameof(UmbracoExamineIndex.CommitCount)] = Index.CommitCount,
                    [nameof(UmbracoExamineIndex.DefaultAnalyzer)] = Index.DefaultAnalyzer.GetType().Name,
                    ["LuceneDirectory"] = Index.GetLuceneDirectory().GetType().Name,
                    [nameof(UmbracoExamineIndex.LuceneIndexFolder)] =
                        Index.LuceneIndexFolder == null
                            ? string.Empty
                            : Index.LuceneIndexFolder.ToString().ToLowerInvariant().TrimStart(IOHelper.MapPath(SystemDirectories.Root).ToLowerInvariant()).Replace("\\", "/").EnsureStartsWith('/'),
                };

                return d;
            }
        }

        
    }
}
