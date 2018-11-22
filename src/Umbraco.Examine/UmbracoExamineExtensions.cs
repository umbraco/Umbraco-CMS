using System;
using System.ComponentModel;
using System.Web;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core;
using Umbraco.Examine.Config;

namespace Umbraco.Examine
{
    public static class UmbracoExamineExtensions
    {
        public static IBooleanOperation Id(this IQuery query, int id)
        {
            var fieldQuery = query.Id(id.ToInvariantString());
            return fieldQuery;
        }

        /// <summary>
        /// Query method to search on parent id
        /// </summary>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IBooleanOperation ParentId(this IQuery query, int id)
        {
            var fieldQuery = query.Field("parentID", id);
            return fieldQuery;
        }

        /// <summary>
        /// Query method to search on node name
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static IBooleanOperation NodeName(this IQuery query, string nodeName)
        {
            var fieldQuery = query.Field("nodeName", (IExamineValue)new ExamineValue(Examineness.Explicit, nodeName));
            return fieldQuery;
        }

        /// <summary>
        /// Query method to search on node name
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public static IBooleanOperation NodeName(this IQuery query, IExamineValue nodeName)
        {
            var fieldQuery = query.Field("nodeName", nodeName);
            return fieldQuery;
        }

        /// <summary>
        /// Query method to search on node type alias
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        public static IBooleanOperation NodeTypeAlias(this IQuery query, string nodeTypeAlias)
        {
            var fieldQuery = query.Field("__NodeTypeAlias", (IExamineValue)new ExamineValue(Examineness.Explicit, nodeTypeAlias));
            return fieldQuery;
        }

        /// <summary>
        /// Query method to search on node type alias
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        public static IBooleanOperation NodeTypeAlias(this IQuery query, IExamineValue nodeTypeAlias)
        {
            var fieldQuery = query.Field("__NodeTypeAlias", nodeTypeAlias);
            return fieldQuery;
        }

        /// <summary>
        /// Used to replace any available tokens in the index path before the lucene directory is assigned to the path
        /// </summary>
        /// <param name="indexSet"></param>
        internal static void ReplaceTokensInIndexPath(this IndexSet indexSet)
        {
            if (indexSet == null) return;
            indexSet.IndexPath = indexSet.IndexPath
                .Replace("{machinename}", NetworkHelper.FileSafeMachineName)
                .Replace("{appdomainappid}", (HttpRuntime.AppDomainAppId ?? string.Empty).ReplaceNonAlphanumericChars(""))
                .EnsureEndsWith('/');
        }
    }
}