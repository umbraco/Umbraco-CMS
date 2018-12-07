using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Umbraco.Examine
{
    /// <summary>
    /// Extension methods for the LuceneIndex
    /// </summary>
    internal static class ExamineExtensions
    {
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
