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

        /// <summary>
        /// Overridden to ensure that the umbraco system field definitions are in place
        /// </summary>
        /// <param name="indexValueTypesFactory"></param>
        /// <returns></returns>
        protected override FieldValueTypeCollection CreateFieldValueTypes(IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
        {
            var keyDef = new FieldDefinition("__key", FieldDefinitionTypes.Raw);
            FieldDefinitionCollection.TryAdd(keyDef);

            return base.CreateFieldValueTypes(indexValueTypesFactory);
        }

        /// <summary>
        /// Ensure some custom values are added to the index
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTransformingIndexValues(IndexingItemEventArgs e)
        {
            base.OnTransformingIndexValues(e);

            if (e.ValueSet.Values.TryGetValue("key", out var key) && e.ValueSet.Values.ContainsKey("__key") == false)
            {
                //double __ prefix means it will be indexed as culture invariant
                e.ValueSet.Values["__key"] = key;
            }

        }

    }
}
