using System;
using Examine;

namespace Umbraco.Web.Search
{
    public class ExamineIndexerModel : ExamineSearcherModel
    {
        public IIndexCriteria IndexCriteria { get; set; }
        
        /// <summary>
        /// The number of docs in the index
        /// </summary>
        public int DocumentCount { get; set; }

        /// <summary>
        /// The number of fields in the index
        /// </summary>
        public int FieldCount { get; set; }

        /// <summary>
        /// The number of documents flagged for deletion in the index
        /// </summary>
        public int DeletionCount { get; set; }

        /// <summary>
        /// Whether or not the indexed is optimized
        /// </summary>
        public bool IsOptimized{ get; set; }

        /// <summary>
        /// Generally will always be true unless someone has created a new non-lucene index
        /// </summary>
        public bool IsLuceneIndex { get; set; }

    }
}