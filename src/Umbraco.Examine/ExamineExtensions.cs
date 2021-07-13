using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Umbraco.Core;
using Version = Lucene.Net.Util.Version;
using Umbraco.Core.Logging;
using System.Threading;

namespace Umbraco.Examine
{
    /// <summary>
    /// Extension methods for the LuceneIndex
    /// </summary>
    public static class ExamineExtensions
    {
        /// <summary>
        /// Matches a culture iso name suffix
        /// </summary>
        /// <remarks>
        /// myFieldName_en-us will match the "en-us"
        /// </remarks>
        internal static readonly Regex CultureIsoCodeFieldNameMatchExpression = new Regex("^(?<FieldName>[_\\w]+)_(?<CultureName>[a-z]{2,3}(-[a-z0-9]{2,4})?)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private static bool _isConfigured = false;
        private static object _configuredInit = null;
        private static object _isConfiguredLocker = new object();

        /// <summary>
        /// Called on startup to configure each index.
        /// </summary>
        /// <remarks>
        /// Configures and unlocks all Lucene based indexes registered with the <see cref="IExamineManager"/>.
        /// </remarks>
        internal static void ConfigureIndexes(this IExamineManager examineManager, IMainDom mainDom, ILogger logger)
        {
            LazyInitializer.EnsureInitialized(
                ref _configuredInit,
                ref _isConfigured,
                ref _isConfiguredLocker,
                () =>
                {
                    examineManager.ConfigureLuceneIndexes(logger, !mainDom.IsMainDom);
                    return null;
                });
        }

        //TODO: We need a public method here to just match a field name against CultureIsoCodeFieldNameMatchExpression

        /// <summary>
        /// Returns all index fields that are culture specific (suffixed)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetCultureFields(this IUmbracoIndex index, string culture)
        {
            var allFields = index.GetFields();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var field in allFields)
            {
                var match = CultureIsoCodeFieldNameMatchExpression.Match(field);
                if (match.Success && culture.InvariantEquals(match.Groups["CultureName"].Value))
                    yield return field;
            }
        }

        /// <summary>
        /// Returns all index fields that are culture specific (suffixed) or invariant
        /// </summary>
        /// <param name="index"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetCultureAndInvariantFields(this IUmbracoIndex index, string culture)
        {
            var allFields = index.GetFields();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var field in allFields)
            {
                var match = CultureIsoCodeFieldNameMatchExpression.Match(field);
                if (match.Success && culture.InvariantEquals(match.Groups["CultureName"].Value))
                {
                    yield return field; //matches this culture field
                }
                else if (!match.Success)
                {
                    yield return field; //matches no culture field (invariant)
                }
                    
            }
        }

        internal static bool TryParseLuceneQuery(string query)
        {
            // TODO: I'd assume there would be a more strict way to parse the query but not that i can find yet, for now we'll
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
        /// Forcibly unlocks all lucene based indexes
        /// </summary>
        /// <remarks>
        /// This is not thread safe, use with care
        /// </remarks>
        internal static void ConfigureLuceneIndexes(this IExamineManager examineManager, ILogger logger, bool disableExamineIndexing)
        {
            foreach (var luceneIndexer in examineManager.Indexes.OfType<LuceneIndex>())
            {
                //We now need to disable waiting for indexing for Examine so that the appdomain is shutdown immediately and doesn't wait for pending
                //indexing operations. We used to wait for indexing operations to complete but this can cause more problems than that is worth because
                //that could end up halting shutdown for a very long time causing overlapping appdomains and many other problems.
                luceneIndexer.WaitForIndexQueueOnShutdown = false;

                if (disableExamineIndexing) continue; //exit if not enabled, we don't need to unlock them if we're not maindom

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
