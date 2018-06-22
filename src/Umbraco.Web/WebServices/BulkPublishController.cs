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
    /// Represents a REST controller used for the publish dialog in order to bulk-publish bulk content items.
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
                content.PublishCulture(); // fixme variants? validation - when this returns null?
                var result = Services.ContentService.SaveAndPublish(content, userId: Security.CurrentUser.Id);
                return Json(new
                    {
                        success = result.Success,
                        message = GetMessageForStatus(result)
                    });
            }
            else
            {
                // fixme variants?
                var result = Services.ContentService.SaveAndPublishBranch(content, includeUnpublished);

                return Json(new
                    {
                        success = result.All(x => x.Success),
                        message = GetMessageForStatuses(result.ToArray(), content)
                    });
            }
        }

        private string GetMessageForStatuses(PublishResult[] statuses, IContent doc)
        {
            //if all are successful then just say it was successful
            if (statuses.All(x => x.Success))
            {
                return Services.TextService.Localize("publish/nodePublishAll", new[] { doc.Name});
            }

            //if they are not all successful the we'll add each error message to the output (one per line)
            var sb = new StringBuilder();
            foreach (var msg in statuses
                .Where(x => ((int)x.Result) >= 10)
                .Select(GetMessageForStatus)
                .Where(msg => msg.IsNullOrWhiteSpace() == false))
            {
                sb.AppendLine(msg.Trim());
            }
            return sb.ToString();
        }

        private string GetMessageForStatus(PublishResult status)
        {
            switch (status.Result)
            {
                case PublishResultType.Success:
                case PublishResultType.SuccessAlready:
                    return Services.TextService.Localize("publish/nodePublish", new[] { status.Content.Name});
                case PublishResultType.FailedPathNotPublished:
                    return Services.TextService.Localize("publish/contentPublishedFailedByParent",
                                   new [] { string.Format("{0} ({1})", status.Content.Name, status.Content.Id) });
                case PublishResultType.FailedHasExpired:
                case PublishResultType.FailedAwaitingRelease:
                case PublishResultType.FailedIsTrashed:
                    return "Cannot publish document with a status of " + status.Result;
                case PublishResultType.FailedCancelledByEvent:
                    return Services.TextService.Localize("publish/contentPublishedFailedByEvent",
                                   new [] { string.Format("'{0}' ({1})", status.Content.Name, status.Content.Id) });
                case PublishResultType.FailedContentInvalid:
                    return Services.TextService.Localize("publish/contentPublishedFailedInvalid",
                                   new []{
                                       string.Format("'{0}' ({1})", status.Content.Name, status.Content.Id),
                                       string.Format("'{0}'", string.Join(", ", status.InvalidProperties.Select(x => x.Alias)))
                                   });
                default:
                    return status.Result.ToString();
            }
        }
    }
}
