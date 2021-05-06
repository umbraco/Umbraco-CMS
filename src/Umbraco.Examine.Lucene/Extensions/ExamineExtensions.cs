// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using System.Threading;
using Examine;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for the LuceneIndex
    /// </summary>
    public static class ExamineExtensions
    {
        private static bool s_isConfigured = false;
        private static object s_configuredInit = null;
        private static object s_isConfiguredLocker = new object();

        /// <summary>
        /// Called on startup to configure each index.
        /// </summary>
        /// <remarks>
        /// Configures and unlocks all Lucene based indexes registered with the <see cref="IExamineManager"/>.
        /// </remarks>
        internal static void ConfigureIndexes(this IExamineManager examineManager, IMainDom mainDom, ILogger<IExamineManager> logger)
            => LazyInitializer.EnsureInitialized(
                ref s_configuredInit,
                ref s_isConfigured,
                ref s_isConfiguredLocker,
                () =>
                {
                    examineManager.ConfigureLuceneIndexes(logger, !mainDom.IsMainDom);
                    return null;
                });

        internal static bool TryParseLuceneQuery(string query)
        {
            // TODO: I'd assume there would be a more strict way to parse the query but not that i can find yet, for now we'll
            // also do this rudimentary check
            if (!query.Contains(":"))
            {
                return false;
            }

            try
            {
                //This will pass with a plain old string without any fields, need to figure out a way to have it properly parse
                var parsed = new QueryParser(LuceneInfo.CurrentVersion, UmbracoExamineFieldNames.NodeNameFieldName, new KeywordAnalyzer()).Parse(query);
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
        private static void ConfigureLuceneIndexes(this IExamineManager examineManager, ILogger<IExamineManager> logger, bool disableExamineIndexing)
        {
            foreach (var luceneIndexer in examineManager.Indexes.OfType<LuceneIndex>())
            {
                if (disableExamineIndexing) continue; //exit if not enabled, we don't need to unlock them if we're not maindom

                //we should check if the index is locked ... it shouldn't be! We are using simple fs lock now and we are also ensuring that
                //the indexes are not operational unless MainDom is true
                var dir = luceneIndexer.GetLuceneDirectory();
                if (IndexWriter.IsLocked(dir))
                {
                    logger.LogDebug("Forcing index {IndexerName} to be unlocked since it was left in a locked state", luceneIndexer.Name);
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
                using (indexer.IndexWriter.IndexWriter.GetReader(false))
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

    }
}
