using System.Linq;
using Umbraco.Core;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Examine;
using UmbracoExamine;
using Lucene.Net.Documents;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Used to wire up events for Examine
    /// </summary>
    public class ExamineEvents : IApplicationEventHandler
    {

        private const string RawFieldPrefix = "__Raw_";

        public void OnApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        public void OnApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
        }

        /// <summary>
        /// Once the app has booted, then bind to the events
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        public void OnApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //do not continue if the app context or database is not ready
            if (!applicationContext.IsConfigured || !applicationContext.DatabaseContext.IsDatabaseConfigured)
                return;

            //TODO: Remove this in 6.1!!! It will not be needed because we've changed the Examine Events entirely since UmbracoExamine is
            // in the core. This is only temporary to get this task completed for 6.0:
            // http://issues.umbraco.org/issue/U4-1530
            MediaService.Saved += MediaService_Saved;
            MediaService.Deleted += MediaService_Deleted;
            MediaService.Moved += MediaService_Moved;
            MediaService.Trashed += MediaService_Trashed;

            ContentService.Saved += ContentService_Saved;
            ContentService.Deleted += ContentService_Deleted;
            ContentService.Moved += ContentService_Moved;
            ContentService.Trashed += ContentService_Trashed;

            //bind to examine events
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

        void ContentService_Trashed(IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            DeleteContent(e.Entity);
        }

        void MediaService_Trashed(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            DeleteMedia(e.Entity);
        }

        void ContentService_Moved(IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            IndexContent(e.Entity);
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            e.DeletedEntities.ForEach(DeleteContent);
        }

        void ContentService_Saved(IContentService sender, Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            //ensure we do not re-index it if it is in the bin            
            e.SavedEntities.Where(x => !x.Trashed).ForEach(IndexContent);
        }

        void MediaService_Moved(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            IndexMedia(e.Entity);
        }

        void MediaService_Deleted(IMediaService sender, Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            e.DeletedEntities.ForEach(DeleteMedia);
        }

        void MediaService_Saved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            //ensure we do not re-index it if it is in the bin            
            e.SavedEntities.Where(x => !x.Trashed).ForEach(IndexMedia);
        }

        private void IndexMedia(IMedia sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "media",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private void DeleteContent(IContent sender)
        {
            ExamineManager.Instance.DeleteFromIndex(
                sender.Id.ToString(),
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private void DeleteMedia(IMedia sender)
        {
            ExamineManager.Instance.DeleteFromIndex(
                sender.Id.ToString(),
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private void IndexContent(IContent sender)
        {
            // Only add to indexes that have SupportUnpublishedContent set to true, this is because any indexer
            // that only supports published content will be notified based on the UmbracoExamineManager.content_AfterUpdateDocumentCache
            // event handler.

            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "content",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>()
                              .Where(x => x.SupportUnpublishedContent && x.EnableDefaultEventHandler));
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