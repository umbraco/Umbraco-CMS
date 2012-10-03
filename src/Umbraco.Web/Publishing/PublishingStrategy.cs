using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Publishing
{
    /// <summary>
    /// Currently acts as an interconnection between the new public api and the legacy api for publishing
    /// </summary>
    internal class PublishingStrategy
    {
        internal PublishingStrategy()
        {
        }

        internal bool Publish(int userId, int contentId)
        {
            var doc = new Document(contentId, true);
            var user = new User(userId);
            return doc.PublishWithResult(user);
        }

        internal bool PublishWithChildrenWithResult(int userId, int contentId)
        {
            var doc = new Document(contentId, true);
            var user = new User(userId);
            return doc.PublishWithChildrenWithResult(user);
        }

        internal void PublishWithSubs(int userId, int contentId)
        {
            var doc = new Document(contentId, true);
            var user = new User(userId);
            doc.PublishWithSubs(user);
        }
    }
}