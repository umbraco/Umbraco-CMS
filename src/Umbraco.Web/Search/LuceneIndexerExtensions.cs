using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Extension methods for the LuceneIndexer
    /// </summary>
    internal static class ExamineExtensions
    {
        /// <summary>
        /// Checks if the index can be read/opened
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="ex">The exception returned if there was an error</param>
        /// <returns></returns>
        public static bool IsHealthy(this LuceneIndexer indexer, out Exception ex)
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
        public static int GetIndexDocumentCount(this LuceneIndexer indexer)
        {
            try
            {
                if (!((indexer.GetSearcher() as LuceneSearcher)?.GetLuceneSearcher() is IndexSearcher searcher))
                    return 0;

                using (searcher)
                using (var reader = searcher.IndexReader)
                {
                    return reader.NumDocs();
                }
            }
            catch (AlreadyClosedException)
            {
                Current.Logger.Warn(typeof(ExamineExtensions), "Cannot get GetIndexDocumentCount, the writer is already closed");
                return 0;
            }
        }

        /// <summary>
        /// Return the total number of fields in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexFieldCount(this LuceneIndexer indexer)
        {
            try
            {
                if (!((indexer.GetSearcher() as LuceneSearcher)?.GetLuceneSearcher() is IndexSearcher searcher))
                    return 0;

                using (searcher)
                using (var reader = searcher.IndexReader)
                {
                    return reader.GetFieldNames(IndexReader.FieldOption.ALL).Count;
                }
            }
            catch (AlreadyClosedException)
            {
                Current.Logger.Warn(typeof(ExamineExtensions), "Cannot get GetIndexFieldCount, the writer is already closed");
                return 0;
            }
        }

        /// <summary>
        /// Check if the index is locked
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        /// <remarks>
        /// If the index does not exist we'll consider it locked
        /// </remarks>
        public static bool IsIndexLocked(this LuceneIndexer indexer)
        {
            return indexer.IndexExists() == false
                   || IndexWriter.IsLocked(indexer.GetLuceneDirectory());
        }

        /// <summary>
        /// The number of documents deleted in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetDeletedDocumentsCount(this LuceneIndexer indexer)
        {
            try
            {
                if (!((indexer.GetSearcher() as LuceneSearcher)?.GetLuceneSearcher() is IndexSearcher searcher))
                    return 0;

                using (searcher)
                using (var reader = searcher.IndexReader)
                {
                    return reader.NumDeletedDocs;
                }
            }
            catch (AlreadyClosedException)
            {
                Current.Logger.Warn(typeof(ExamineExtensions), "Cannot get GetDeletedDocumentsCount, the writer is already closed");
                return 0;
            }
        }
    }
}
