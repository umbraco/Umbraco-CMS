using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using umbraco;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used for the publish dialog in order to publish bulk items at once
    /// </summary>
    [ValidateMvcAngularAntiForgeryToken]
    public class BulkPublishController : UmbracoAuthorizedController
    {
        /// <summary>
        /// Publishes an document
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="publishDescendants">true to publish descendants as well</param>
        /// <param name="includeUnpublished">true to publish documents that are unpublished</param>
        /// <returns>A Json array containing objects with the child id's of the document and it's current published status</returns>
        [HttpPost]
        public JsonResult PublishDocument(int documentId, bool publishDescendants, bool includeUnpublished)
        {
            var content = Services.ContentService.GetById(documentId);
            var doc = new Document(content);
            //var contentService = (ContentService) Services.ContentService;
            if (publishDescendants == false)
            {
                //var result = contentService.SaveAndPublishInternal(content);
                var result = doc.SaveAndPublish(UmbracoUser.Id);
                return Json(new
                    {
                        success = result.Success,
                        message = GetMessageForStatus(result.Result)
                    });
            }
            else
            {
                /*var result = ((ContentService) Services.ContentService)
                    .PublishWithChildrenInternal(content, UmbracoUser.Id, includeUnpublished)
                    .ToArray();*/
                var result = doc.PublishWithSubs(UmbracoUser.Id, includeUnpublished);
                return Json(new
                    {
                        success = result.All(x => x.Success),
                        message = GetMessageForStatuses(result.Select(x => x.Result), content)
                    });
            }
        }

        private string GetMessageForStatuses(IEnumerable<PublishStatus> statuses, IContent doc)
        {
            //if all are successful then just say it was successful
            if (statuses.All(x => ((int) x.StatusType) < 10))
            {
                return ui.Text("publish", "nodePublishAll", doc.Name, UmbracoUser);
            }

            //if they are not all successful the we'll add each error message to the output (one per line)
            var sb = new StringBuilder();
            foreach (var msg in statuses
                .Where(x => ((int)x.StatusType) >= 10)
                .Select(GetMessageForStatus)
                .Where(msg => msg.IsNullOrWhiteSpace() == false))
            {
                sb.AppendLine(msg.Trim());
            }
            return sb.ToString();
        }

        private string GetMessageForStatus(PublishStatus status)
        {
            switch (status.StatusType)
            {
                case PublishStatusType.Success:
                case PublishStatusType.SuccessAlreadyPublished:
                    return ui.Text("publish", "nodePublish", status.ContentItem.Name, UmbracoUser);
                case PublishStatusType.FailedPathNotPublished:
                    return ui.Text("publish", "contentPublishedFailedByParent", 
                                   string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id), UmbracoUser);
                case PublishStatusType.FailedHasExpired:                    
                case PublishStatusType.FailedAwaitingRelease:
                case PublishStatusType.FailedIsTrashed:
                    return "Cannot publish document with a status of " + status.StatusType;
                case PublishStatusType.FailedCancelledByEvent:
                    return ui.Text("publish", "contentPublishedFailedByEvent",
                                   string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id), UmbracoUser);
                case PublishStatusType.FailedContentInvalid:
                    return ui.Text("publish", "contentPublishedFailedInvalid",
                                   new []{
                                       string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id), 
                                       string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                   }, 
                                   UmbracoUser);  
                default:
                    return status.StatusType.ToString();
            }
        }
    }
}