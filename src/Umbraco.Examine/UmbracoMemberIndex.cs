using System.Collections.Generic;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Analysis;
using Umbraco.Core.Logging;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{

    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndex : UmbracoExamineIndex
    {
        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="analyzer"></param>
        public UmbracoMemberIndex(
            string name,
            FieldDefinitionCollection fieldDefinitions,
            Directory luceneDirectory,
            Analyzer analyzer,
            IProfilingLogger profilingLogger,
            IValueSetValidator validator = null) :
            base(name, luceneDirectory, fieldDefinitions, analyzer, profilingLogger, validator)
        {
        }
        
    }
}
