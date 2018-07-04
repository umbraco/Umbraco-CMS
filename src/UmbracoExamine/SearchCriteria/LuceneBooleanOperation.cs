using Examine.SearchCriteria;
using Lucene.Net.Search;

namespace UmbracoExamine.SearchCriteria
{
    /// <summary>
    /// An implementation of the fluent API boolean operations
    /// </summary>
    public class LuceneBooleanOperation : IBooleanOperation
    {
        private LuceneSearchCriteria search;

        internal LuceneBooleanOperation(LuceneSearchCriteria search)
        {
            this.search = search;
        }

        #region IBooleanOperation Members

        /// <summary>
        /// Sets the next operation to be AND
        /// </summary>
        /// <returns></returns>
        public IQuery And()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST);
        }

        /// <summary>
        /// Sets the next operation to be OR
        /// </summary>
        /// <returns></returns>
        public IQuery Or()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.SHOULD);
        }

        /// <summary>
        /// Sets the next operation to be NOT
        /// </summary>
        /// <returns></returns>
        public IQuery Not()
        {
            return new LuceneQuery(this.search, BooleanClause.Occur.MUST_NOT);
        }

        /// <summary>
        /// Compiles this instance for fluent API conclusion
        /// </summary>
        /// <returns></returns>
        public ISearchCriteria Compile()
        {
            if (!string.IsNullOrEmpty(this.search.SearchIndexType))
            {
                var query = this.search.query;

                this.search.query = new BooleanQuery();
                this.search.query.Add(query, BooleanClause.Occur.MUST);

                //this.search.query.Add(this.search.queryParser.Parse("(" + query.ToString() + ")"), BooleanClause.Occur.MUST);

                this.search.FieldInternal(LuceneExamineIndexer.IndexTypeFieldName, new ExamineValue(Examineness.Explicit, this.search.SearchIndexType.ToString().ToLower()), BooleanClause.Occur.MUST);
            }
            
            return this.search;
        }

        #endregion
    }
}
