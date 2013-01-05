using System;
using System.IO;
using System.Linq;
using System.Security;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using UmbracoExamine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;


namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
	public class UmbracoExamineSearcher : LuceneSearcher
    {

        #region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
        public UmbracoExamineSearcher()
            : base()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
		[SecuritySafeCritical]
		public UmbracoExamineSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {
        }

		/// <summary>
		/// Constructor to allow for creating an indexer at runtime
		/// </summary>
		/// <param name="luceneDirectory"></param>
		/// <param name="analyzer"></param>
		[SecuritySafeCritical]
		public UmbracoExamineSearcher(Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
			: base(luceneDirectory, analyzer)
		{
		}

		#endregion

        /// <summary>
        /// Override in order to set the nodeTypeAlias field name of the underlying SearchCriteria to __NodeTypeAlias
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultOperation"></param>
        /// <returns></returns>
        public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            var criteria = base.CreateSearchCriteria(type, defaultOperation) as LuceneSearchCriteria;
            criteria.NodeTypeAliasField = UmbracoContentIndexer.NodeTypeAliasFieldName;
            return criteria;
        }

        /// <summary>
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
        /// </summary>
        /// <returns></returns>
        protected internal override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields
                .Where(x => x != UmbracoContentIndexer.IndexPathFieldName)
                .Where(x => x != UmbracoContentIndexer.NodeTypeAliasFieldName)
                .ToArray();
        }		
    }
}
