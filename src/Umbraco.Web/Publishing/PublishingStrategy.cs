using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Publishing
{
    /// <summary>
    /// Currently acts as an interconnection between the new public api and the legacy api for publishing
    /// </summary>
    internal class PublishingStrategy : IPublishingStrategy
    {
        internal PublishingStrategy()
        {
        }

        public bool Publish(IContent content, int userId)
        {
            //Fire BeforePublish event
            /*PublishEventArgs e = new PublishEventArgs();
            FireBeforePublish(e);*/
            
            //Create new (unpublished) version - Guid is returned
            //Log Publish
            //Update all cmsDocument entries with Id to newest=0
            //Insert new version in cmsDocument table
            //Set Release and Expire date on newly created version

            // Update xml in db using the new document (has correct version date)
            //newDoc.XmlGenerate(new XmlDocument());

            //Fire AfterPublish event
            //FireAfterPublish(e);

            //Updating the cache is not done in the Document-Publish methods, so this part should be added
            //global::umbraco.library.UpdateDocumentCache(doc.Id);

            int contentId = content.Id;

            var doc = new Document(contentId, true);
            var user = new User(userId);
            
            return doc.PublishWithResult(user);
        }

        public bool PublishWithChildren(IEnumerable<IContent> children, int userId)
        {
            int contentId = children.Last().Id;
            var doc = new Document(contentId, true);
            var user = new User(userId);
            return doc.PublishWithChildrenWithResult(user);
        }

        public void PublishWithSubs(IContent content, int userId)
        {
            int contentId = content.Id;
            var doc = new Document(contentId, true);
            var user = new User(userId);
            doc.PublishWithSubs(user);
        }

        public bool UnPublish(IContent content, int userId)
        {
            return false;
        }
    }
}