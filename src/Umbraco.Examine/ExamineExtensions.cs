using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Umbraco.Core.Logging;

namespace Umbraco.Examine
{
    /// <summary>
    /// Extension methods for the LuceneIndex
    /// </summary>
    internal static class ExamineExtensions
    {
        /// <summary>
        /// Forcibly unlocks all lucene based indexes
        /// </summary>
        /// <remarks>
        /// This is not thread safe, use with care
        /// </remarks>
        internal static void UnlockLuceneIndexes(this IExamineManager examineManager, ILogger logger)
        {
            foreach (var luceneIndexer in examineManager.Indexes.OfType<LuceneIndex>())
            {
                //We now need to disable waiting for indexing for Examine so that the appdomain is shutdown immediately and doesn't wait for pending
                //indexing operations. We used to wait for indexing operations to complete but this can cause more problems than that is worth because
                //that could end up halting shutdown for a very long time causing overlapping appdomains and many other problems.
                luceneIndexer.WaitForIndexQueueOnShutdown = false;

                //we should check if the index is locked ... it shouldn't be! We are using simple fs lock now and we are also ensuring that
                //the indexes are not operational unless MainDom is true
                var dir = luceneIndexer.GetLuceneDirectory();
                if (IndexWriter.IsLocked(dir))
                {
                    logger.Info(typeof(ExamineExtensions), "Forcing index {IndexerName} to be unlocked since it was left in a locked state", luceneIndexer.Name);
                    IndexWriter.Unlock(dir);
                }
            }
        }

        /// <summary>
        /// Checks if the index can be read/opened
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="ex">The exception returned if there was an error</param>
        /// <returns></returns>
        public static bool IsHealthy(this LuceneIndex indexer, out Exception ex)
        {
            try
            {
                using (indexer.GetIndexWriter().GetReader())
                {
                    ex = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        /// <summary>
        /// Return the number of indexed documents in Lucene
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexDocumentCount(this LuceneIndex indexer)
        {
            if (!((indexer.GetSearcher() as LuceneSearcher)?.GetLuceneSearcher() is IndexSearcher searcher))
                return 0;

            using (searcher)
            using (var reader = searcher.IndexReader)
            {
                return reader.NumDocs();
            }
        }

        /// <summary>
        /// Return the total number of fields in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexFieldCount(this LuceneIndex indexer)
        {
            if (!((indexer.GetSearcher() as LuceneSearcher)?.GetLuceneSearcher() is IndexSearcher searcher))
                return 0;

            using (searcher)
            using (var reader = searcher.IndexReader)
            {
                return reader.GetFieldNames(IndexReader.FieldOption.ALL).Count;
            }
        }

    }
}
