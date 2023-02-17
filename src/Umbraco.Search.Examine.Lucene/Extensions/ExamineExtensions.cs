// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Examine;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis.Core;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for the LuceneIndex
/// </summary>
public static class ExamineExtensions
{
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
            Query? parsed = new QueryParser(LuceneInfo.CurrentVersion, UmbracoExamineFieldNames.NodeNameFieldName, new KeywordAnalyzer()).Parse(query);
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
    ///     Checks if the index can be read/opened
    /// </summary>
    /// <param name="indexer"></param>
    /// <param name="ex">The exception returned if there was an error</param>
    /// <returns></returns>
    public static bool IsHealthy(this LuceneIndex indexer, [MaybeNullWhen(true)] out Exception ex)
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
