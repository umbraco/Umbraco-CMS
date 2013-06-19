using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Examine.Providers;
using Umbraco.Core;
using umbraco.BusinessLogic;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Used to wire up events for Examine
    /// </summary>
    public class ExamineEvents : IApplicationStartupHandler
    {

        private const string RawFieldPrefix = "__Raw_";

        public ExamineEvents()
            : base()
        {
            var contentIndexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoContentIndexer;
            if (contentIndexer != null)
            {
                contentIndexer.GatheringNodeData += ContentIndexerGatheringNodeData;
                contentIndexer.DocumentWriting += IndexerDocumentWriting;
            }
            var memberIndexer = ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"] as UmbracoMemberIndexer;
            if (memberIndexer != null)
            {
                memberIndexer.DocumentWriting += IndexerDocumentWriting;
            }
        }

        /// <summary>
        /// This checks if any user data might be xml/html, if so we will duplicate the field and store the raw value
        /// so we can retreive the raw value when required.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// This is regarding this issue: http://issues.umbraco.org/issue/U4-644
        /// The underlying UmbracoContentIndexer strips the HTML values before this event is even fired
        /// so we need to check in the underlying 'node' document for the value.
        /// </remarks>
        static void ContentIndexerGatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            var indexer = sender as UmbracoContentIndexer;
            if (indexer == null) return;

            //loop through each field that is defined as a UserField for the index
            foreach (var field in indexer.IndexerData.UserFields)
            {
                if (e.Fields.ContainsKey(field.Name))
                {
                    //get the original value from the node
                    var node = e.Node.Descendants(field.Name).FirstOrDefault();
                    if (node == null) continue;
                    
                    //check if the node value has html
                    if (XmlHelper.CouldItBeXml(node.Value))
                    {
                        //First save the raw value to a raw field, we will change the policy of this field by detecting the prefix later
                        e.Fields[RawFieldPrefix + field.Name] = node.Value;
                    }
                }
            }

        }

        /// <summary>
        /// Event handler to create a lower cased version of the node name, this is so we can support case-insensitive searching and still
        /// use the Whitespace Analyzer. This also ensures the 'Raw' values are added to the document.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void IndexerDocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            //This ensures that the special __Raw_ fields are indexed
            var d = e.Document;
            foreach (var f in e.Fields.Where(x => x.Key.StartsWith(RawFieldPrefix)))
            {
                d.Add(new Field(
                          f.Key,
                          f.Value,
                          Field.Store.YES,
                          Field.Index.NO, //don't index this field, we never want to search by it 
                          Field.TermVector.NO));
            }

            //add the lower cased version
            if (e.Fields.Keys.Contains("nodeName"))
            {                
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