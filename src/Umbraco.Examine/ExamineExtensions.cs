using System;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Umbraco.Examine
{
    /// <summary>
    /// Extension methods for the LuceneIndex
    /// </summary>
    internal static class ExamineExtensions
    {
        public static bool TryParseLuceneQuery(string query)
        {
            //TODO: I'd assume there would be a more strict way to parse the query but not that i can find yet, for now we'll
            // also do this rudimentary check
            if (!query.Contains(":"))
                return false;

            try
            {
                //This will pass with a plain old string without any fields, need to figure out a way to have it properly parse
                var parsed = new QueryParser(Version.LUCENE_30, "nodeName", new KeywordAnalyzer()).Parse(query);
                return true;
            }
            catch (ParseException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
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
