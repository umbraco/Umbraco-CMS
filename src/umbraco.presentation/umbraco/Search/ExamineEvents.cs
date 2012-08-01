using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;
using umbraco.businesslogic;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Used to wire up events for Examine
    /// </summary>
    public class ExamineEvents : ApplicationStartupHandler
    {

        public ExamineEvents()
            : base()
        {
            var contentIndexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoContentIndexer;
            if (contentIndexer != null)
            {
                contentIndexer.DocumentWriting += new EventHandler<Examine.LuceneEngine.DocumentWritingEventArgs>(indexer_DocumentWriting);
            }
            var memberIndexer = ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"] as UmbracoMemberIndexer;
            if (memberIndexer != null)
            {
                memberIndexer.DocumentWriting += new EventHandler<Examine.LuceneEngine.DocumentWritingEventArgs>(indexer_DocumentWriting);
            }
        }

        /// <summary>
        /// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
        /// use the Whitespace Analyzer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void indexer_DocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            if (e.Fields.Keys.Contains("nodeName"))
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

    }
}