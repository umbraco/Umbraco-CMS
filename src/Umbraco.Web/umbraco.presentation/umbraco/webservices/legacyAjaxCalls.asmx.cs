using System;
using System.Web;
using System.Web.Services;
using System.ComponentModel;
using System.Web.Script.Services;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.WebServices;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for legacyAjaxCalls
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class legacyAjaxCalls : UmbracoAuthorizedWebService
    {
        private IUser _currentUser;

        /// <summary>
        /// method to accept a string value for the node id. Used for tree's such as python
        /// and xslt since the file names are the node IDs
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="alias"></param>
        /// <param name="nodeType"></param>
        [WebMethod]
        [ScriptMethod]
        public void Delete(string nodeId, string alias, string nodeType)
        {
            if (!AuthorizeRequest())
                return;

            //U4-2686 - alias is html encoded, make sure to decode
            alias = HttpUtility.HtmlDecode(alias);

            //check which parameters to pass depending on the types passed in
            int intNodeId;
            if (nodeType == "memberGroups")
            {
                 LegacyDialogHandler.Delete(
                    new HttpContextWrapper(HttpContext.Current),
                    Security.CurrentUser,
                    nodeType, 0, nodeId);
            }
            else if (int.TryParse(nodeId, out intNodeId) && nodeType != "member") // Fix for #26965 - numeric member login gets parsed as nodeId
            {
                LegacyDialogHandler.Delete(
                    new HttpContextWrapper(HttpContext.Current),
                    Security.CurrentUser,
                    nodeType, intNodeId, alias);
            }
            else
            {
                LegacyDialogHandler.Delete(
                    new HttpContextWrapper(HttpContext.Current),
                    Security.CurrentUser,
                    nodeType, 0, nodeId);
            }
        }

        /// <summary>
        /// Permanently deletes a document/media object.
        /// Used to remove an item from the recycle bin.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="nodeType"></param>
        [WebMethod]
        [ScriptMethod]
        public void DeleteContentPermanently(string nodeId, string nodeType)
        {
            int intNodeId;
            if (int.TryParse(nodeId, out intNodeId))
            {
                switch (nodeType)
                {
                    case "media":
                    case "mediaRecycleBin":
                        //ensure user has access to media
                        AuthorizeRequest(Constants.Applications.Media.ToString(), true);
                        var media = Current.Services.MediaService.GetById(intNodeId);
                        if (media != null)
                            Current.Services.MediaService.Delete(media);
                        break;
                    case "content":
                    case "contentRecycleBin":
                    default:
                        //ensure user has access to content
                        AuthorizeRequest(Constants.Applications.Content.ToString(), true);
                        var content = Current.Services.ContentService.GetById(intNodeId);
                        if (content != null)
                            Current.Services.ContentService.Delete(content);
                        break;
                }
            }
            else
            {
                throw new ArgumentException("The nodeId argument could not be parsed to an integer");
            }
        }

        [WebMethod]
        [ScriptMethod]
        public void DisableUser(int userId)
        {
            AuthorizeRequest(Constants.Applications.Users.ToString(), true);

            var user = Services.UserService.GetUserById(userId);
            if (user == null) return;

            user.IsApproved = false;
            Services.UserService.Save(user);
        }

        [WebMethod]
        [ScriptMethod]
        public string NiceUrl(int nodeId)
        {

            AuthorizeRequest(true);

            return library.NiceUrl(nodeId);
        }

        [WebMethod]
        [ScriptMethod]
        public string ProgressStatus(string Key)
        {
            AuthorizeRequest(true);

            return Application[Context.Request.GetItemAsString("key")].ToString();
        }
    }
}
