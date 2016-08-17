using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

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

            if (publishDescendants == false)
            {
                var result = Services.ContentService.SaveAndPublishWithStatus(content, Security.CurrentUser.Id);
                return Json(new
                    {
                        success = result.Success,
                        message = GetMessageForStatus(result.Result)
                    });
            }
            else
            {
                var result = Services.ContentService
                    .PublishWithChildrenWithStatus(content, Security.CurrentUser.Id, includeUnpublished)
                    .ToArray();

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
            if (statuses.All(x => x.StatusType.IsSuccess()))
            {
                return Services.TextService.Localize("publish/nodePublishAll", new[] { doc.Name});
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
                    return Services.TextService.Localize("publish/nodePublish", new[] { status.ContentItem.Name});
                case PublishStatusType.FailedPathNotPublished:
                    return Services.TextService.Localize("publish/contentPublishedFailedByParent", 
                                   new [] { string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id) });
                case PublishStatusType.FailedHasExpired:                    
                case PublishStatusType.FailedAwaitingRelease:
                case PublishStatusType.FailedIsTrashed:
                    return "Cannot publish document with a status of " + status.StatusType;
                case PublishStatusType.FailedCancelledByEvent:
                    return Services.TextService.Localize("publish/contentPublishedFailedByEvent",
                                   new [] { string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id) });
                case PublishStatusType.FailedContentInvalid:
                    return Services.TextService.Localize("publish/contentPublishedFailedInvalid",
                                   new []{
                                       string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id), 
                                       string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                   });  
                default:
                    return status.StatusType.ToString();
            }
        }
    }
}