using System.Linq;
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
            ContentService.Saved += ContentService_Saved;
            ContentService.Deleted += ContentService_Deleted;
            ContentService.Moved += ContentService_Moved;

            //bind to examine events
            var contentIndexer = ExamineManager.Instance.IndexProviderCollection["InternalIndexer"] as UmbracoContentIndexer;
            if (contentIndexer != null)
            {
                contentIndexer.DocumentWriting += indexer_DocumentWriting;
            }
            var memberIndexer = ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"] as UmbracoMemberIndexer;
            if (memberIndexer != null)
            {
                memberIndexer.DocumentWriting += indexer_DocumentWriting;
            }
        }

        void ContentService_Moved(IContentService sender, Umbraco.Core.Events.MoveEventArgs<IContent> e)
        {
            IndexConent(e.Entity);
        }

        void ContentService_Deleted(IContentService sender, Umbraco.Core.Events.DeleteEventArgs<IContent> e)
        {
            e.DeletedEntities.ForEach(
                content =>
                ExamineManager.Instance.DeleteFromIndex(
                    content.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        void ContentService_Saved(IContentService sender, Umbraco.Core.Events.SaveEventArgs<IContent> e)
        {
            e.SavedEntities.ForEach(IndexConent);
        }

        void MediaService_Moved(IMediaService sender, Umbraco.Core.Events.MoveEventArgs<IMedia> e)
        {
            IndexMedia(e.Entity);
        }

        void MediaService_Deleted(IMediaService sender, Umbraco.Core.Events.DeleteEventArgs<IMedia> e)
        {
            e.DeletedEntities.ForEach(
                media =>
                ExamineManager.Instance.DeleteFromIndex(
                    media.Id.ToString(),
                    ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler)));
        }

        void MediaService_Saved(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
        {
            e.SavedEntities.ForEach(IndexMedia);
        }

        private void IndexMedia(IMedia sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "media",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
        }

        private void IndexConent(IContent sender)
        {
            ExamineManager.Instance.ReIndexNode(
                sender.ToXml(), "content",
                ExamineManager.Instance.IndexProviderCollection.OfType<BaseUmbracoIndexer>().Where(x => x.EnableDefaultEventHandler));
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