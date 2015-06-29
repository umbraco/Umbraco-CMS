using System;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Extension methods for the LuceneIndexer
    /// </summary>
    internal static class ExamineExtensions
    {
        public static LuceneSearcher GetSearcherForIndexer(this LuceneIndexer indexer)
        {
            var indexSet = indexer.IndexSetName;
            var searcher = ExamineManager.Instance.SearchProviderCollection.OfType<LuceneSearcher>()
                                         .FirstOrDefault(x => x.IndexSetName == indexSet);
            if (searcher == null)
                throw new InvalidOperationException("No searcher assigned to the index set " + indexer.IndexSetName);
            return searcher;
        }

        private static IndexReader GetIndexReaderForSearcher(this BaseLuceneSearcher searcher)
        {
            var indexSearcher = searcher.GetSearcher() as IndexSearcher;
            if (indexSearcher == null)
                throw new InvalidOperationException("The index searcher is not of type " + typeof(IndexSearcher) + " cannot execute this method");
            return indexSearcher.GetIndexReader();
        }

        /// <summary>
        /// Return the number of indexed documents in Lucene
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexDocumentCount(this LuceneIndexer indexer)
        {
            return indexer.GetSearcherForIndexer().GetIndexReaderForSearcher().NumDocs();
        }

        /// <summary>
        /// Return the total number of fields in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexFieldCount(this LuceneIndexer indexer)
        {
            return indexer.GetSearcherForIndexer().GetIndexReaderForSearcher().GetFieldNames(IndexReader.FieldOption.ALL).Count;
        }

        /// <summary>
        /// Returns true if the index is optimized or not
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static bool IsIndexOptimized(this LuceneIndexer indexer)
        {
            return indexer.GetSearcherForIndexer().GetIndexReaderForSearcher().IsOptimized();
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
            return !indexer.IndexExists()
                   || IndexWriter.IsLocked(indexer.GetSearcherForIndexer().GetIndexReaderForSearcher().Directory());
        }

        /// <summary>
        /// The number of documents deleted in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetDeletedDocumentsCount(this LuceneIndexer indexer)
        {
            return indexer.GetSearcherForIndexer().GetIndexReaderForSearcher().NumDeletedDocs();
        }
    }
}