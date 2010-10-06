using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Used to wire up events for Examine
    /// </summary>
    public class ExamineEvents : ApplicationBase
    {

        public ExamineEvents()
            : base()
        {
            var indexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoExamineIndexer;
            if (indexer != null)
            {
                indexer.DocumentWriting += new EventHandler<Examine.LuceneEngine.DocumentWritingEventArgs>(indexer_DocumentWriting);
            }
            
        }

        void indexer_DocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            if (e.Fields[UmbracoExamineIndexer.IndexTypeFieldName] == IndexTypes.Content && e.Fields.Keys.Contains("nodeName"))
            {
                //add the lower cased version
                e.Document.Add(new Field("__nodeName",
                                        e.Fields["nodeName"].ToLower(),
                                        Field.Store.YES,
                                        Field.Index.ANALYZED,
                                        Field.TermVector.NO
                                        ));
            }
        }


        /// <summary>
        /// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
        /// use the Whitespace Analyzer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void ExamineEvents_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        //{
        //    if (e.IndexType == UmbracoExamine.IndexTypes.Content && e.Fields.Keys.Contains("nodeName"))
        //    {
        //        //add the lower cased version
        //        e.Fields.Add("__nodeName", e.Fields["nodeName"].ToLower());
        //    }
        //}

    }
}