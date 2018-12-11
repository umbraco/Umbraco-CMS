//using System;
//using System.ComponentModel;
//using System.IO;
//using System.Linq;
//using Umbraco.Core;
//using Examine.LuceneEngine.Providers;
//using Lucene.Net.Analysis;
//using Lucene.Net.Index;
//using Umbraco.Core.Composing;
//using Umbraco.Examine.Config;
//using Directory = Lucene.Net.Store.Directory;
//using Examine.LuceneEngine;

//namespace Umbraco.Examine
//{
//    /// <summary>
//    /// An Examine searcher which uses Lucene.Net as the
//    /// </summary>
//    public class UmbracoExamineSearcher : LuceneSearcher
//    {
//        /// <summary>
//        /// Constructor to allow for creating an indexer at runtime
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="luceneDirectory"></param>
//        /// <param name="analyzer"></param>
//        public UmbracoExamineSearcher(string name, Directory luceneDirectory, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection)
//            : base(name, luceneDirectory, analyzer, fieldValueTypeCollection)
//        {
//        }

//        /// <inheritdoc />
//        public UmbracoExamineSearcher(string name, IndexWriter writer, Analyzer analyzer, FieldValueTypeCollection fieldValueTypeCollection)
//            : base(name, writer, analyzer, fieldValueTypeCollection)
//        {
//        }

//        /// <summary>
//        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
//        /// </summary>
//        /// <returns></returns>
//        public override string[] GetAllIndexedFields()
//        {
//            var fields = base.GetAllIndexedFields();
//            return fields
//                .Where(x => x != UmbracoExamineIndexer.IndexPathFieldName)
//                .Where(x => x != LuceneIndex.ItemTypeFieldName)
//                .ToArray();
//        }

//    }
//}
