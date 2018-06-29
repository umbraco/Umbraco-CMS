using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Examine;

namespace Umbraco.Web.Search
{
    [DataContract(Name = "indexer", Namespace = "")]
    public class ExamineIndexerModel : ExamineSearcherModel
    {
        [DataMember(Name = "fieldDefinitions")]
        public IEnumerable<FieldDefinition> FieldDefinitions { get; set; }

        /// <summary>
        /// The number of docs in the index
        /// </summary>
        [DataMember(Name = "documentCount")]
        public int DocumentCount { get; set; }

        /// <summary>
        /// The number of fields in the index
        /// </summary>
        [DataMember(Name = "fieldCount")]
        public int FieldCount { get; set; }
        
        /// <summary>
        /// Generally will always be true unless someone has created a new non-lucene index
        /// </summary>
        [DataMember(Name = "isLuceneIndex")]
        public bool IsLuceneIndex { get; set; }
    }
}
