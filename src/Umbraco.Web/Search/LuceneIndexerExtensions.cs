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

        /// <summary>
        /// Return the number of indexed documents in Lucene
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexDocumentCount(this LuceneIndexer indexer)
        {
            using (var reader = indexer.GetIndexWriter().GetReader())
            {
                return reader.NumDocs();
            }
        }

        /// <summary>
        /// Return the total number of fields in the index
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static int GetIndexFieldCount(this LuceneIndexer indexer)
        {
            using (var reader = indexer.GetIndexWriter().GetReader())
            {
                return reader.GetFieldNames(IndexReader.FieldOption.ALL).Count;
            }
        }

        /// <summary>
        /// Returns true if the index is optimized or not
        /// </summary>
        /// <param name="indexer"></param>
        /// <returns></returns>
        public static bool IsIndexOptimized(this LuceneIndexer indexer)
        {
            using (var reader = indexer.GetIndexWriter().GetReader())
            {
                return reader.IsOptimized();
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
            using (var reader = indexer.GetIndexWriter().GetReader())
            {
                return reader.NumDeletedDocs();
            }
        }
    }
}