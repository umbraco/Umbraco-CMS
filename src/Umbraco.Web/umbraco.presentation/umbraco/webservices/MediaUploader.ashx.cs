using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.media;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace umbraco.presentation.umbraco.webservices
{
    [Obsolete("This should no longer be used, use the WebApi methods to upload media")]
    public class MediaUploader : IHttpHandler
    {
        protected User AuthenticatedUser { get; set; }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            MediaResponse response = null;

            var action = context.Request["action"];

            if (IsValidRequest(context) && !string.IsNullOrEmpty(action))
            {
                switch (action.ToLower())
                {
                    case "config":
                        response = ProcessConfigRequest(context);
                        break;
                    case "folderlist":
                        response = ProcessFolderListRequest(context);
                        break;
                    case "upload":
                        response = ProcessUploadRequest(context);
                        break;
                }
            }

            // Set success flag
            if (response != null)
                response.success = true;
            else
                response = new MediaResponse { success = false };

            context.Response.Clear();
            context.Response.Charset = "UTF-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetAllowResponseInBrowserHistory(true);

            var format = context.Request["format"];
            switch (format)
            {
                case "json":
                    // Format as JSON 
                    // Weirdly, this should be set to text/html to make it respond as json according to:
                    // http://stackoverflow.com/questions/6934393/resource-interpreted-as-document-but-transferred-with-mime-type-application-jso
                    context.Response.ContentType = @"text/html";

                    context.Response.Write(new JavaScriptSerializer().Serialize(response));

                    break;
                default:
                    // Format as XML
                    context.Response.ContentType = @"text/xml";

                    var serializer = new XmlSerializer(response.GetType());
                    serializer.Serialize(context.Response.OutputStream, response);

                    break;
            }
            

            context.Response.End();
        }

        public ConfigResponse ProcessConfigRequest(HttpContext context)
        {
            return new ConfigResponse
            {
                displayName = new User(context.Request["username"]).Name,
                umbracoPath = VirtualPathUtility.ToAbsolute(GlobalSettings.Path),
                maxRequestLength = GetMaxRequestLength()
            };
        }

        public FolderListResponse ProcessFolderListRequest(HttpContext context)
        {
            var response = new FolderListResponse
            {
                folder = new FolderListItem()
            };

            var startMediaId = AuthenticatedUser.StartMediaId;
            if (startMediaId < 1)
            {
                response.folder.id = -1;
                response.folder.name = "Media";

                CreateMediaTree(Media.GetRootMedias(), response.folder);
            }
            else
            {
                var root = new Media(startMediaId);

                response.folder.id = root.Id;
                response.folder.name = root.Text;

                CreateMediaTree(root.Children, response.folder);
            }

            return response;
        }

        public UploadResponse ProcessUploadRequest(HttpContext context)
        {
            int parentNodeId;
            if (int.TryParse(context.Request["parentNodeId"], out parentNodeId) && context.Request.Files.Count > 0)
            {
                try
                {
                    var parentNode = new Media(parentNodeId);
                    // Check FilePath
                    if (!string.IsNullOrEmpty(context.Request["path"]))
                    {
                        var pathParts = context.Request["path"].Trim('/').Split('/');
                        
                        foreach (var pathPart in pathParts.Where(part => string.IsNullOrWhiteSpace(part) == false))
                                parentNode = GetOrCreateFolder(parentNode, pathPart);

                        parentNodeId = parentNode.Id;
                    }

                    // Check whether to replace existing
                    bool parsed;
                    var replaceExisting = (context.Request["replaceExisting"] == "1" || (bool.TryParse(context.Request["replaceExisting"], out parsed) && parsed));

                    // loop through uploaded files
                    for (var j = 0; j < context.Request.Files.Count; j++)
                    {
                        // get the current file
                        var uploadFile = context.Request.Files[j];

                        //Are we allowed to upload this?
                        var ext = uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf('.') + 1).ToLower();
                        if (UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(ext))
                        {
                            LogHelper.Warn<MediaUploader>("Cannot upload file " + uploadFile.FileName + ", it is not approved in `disallowedUploadFiles` in ~/config/UmbracoSettings.config");
                            continue;
                        }

                        using (var inputStream = uploadFile.InputStream)
                        {
                            // if there was a file uploded
                            if (uploadFile.ContentLength > 0)
                            {
                                // Ensure we get the filename without the path in IE in intranet mode 
                                // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
                                var fileName = uploadFile.FileName;
                                if (fileName.LastIndexOf(@"\") > 0)
                                    fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

                                fileName = Umbraco.Core.IO.IOHelper.SafeFileName(fileName);

                                var postedMediaFile = new PostedMediaFile
                                {
                                    FileName = fileName,
                                    DisplayName = context.Request["name"],
                                    ContentType = uploadFile.ContentType,
                                    ContentLength = uploadFile.ContentLength,
                                    InputStream = inputStream,
                                    ReplaceExisting = replaceExisting
                                };

                                // Get concrete MediaFactory
                                var factory = MediaFactory.GetMediaFactory(parentNodeId, postedMediaFile, AuthenticatedUser);

                                // Handle media Item
                                var media = factory.HandleMedia(parentNodeId, postedMediaFile, AuthenticatedUser);
                            }
                        }
                    }

                    var scripts = new ClientTools(new Page());
                    scripts.SyncTree(parentNode.Path, true);

                    // log succes
                    LogHelper.Info<MediaUploader>(string.Format("Success uploading to parent {0}", parentNodeId));
                }
                catch (Exception e)
                {
                    // log error
                    LogHelper.Error<MediaUploader>(string.Format("Error uploading to parent {0}", parentNodeId), e);
                }
            }
            else
            {
                // log error
                LogHelper.Warn<MediaUploader>(string.Format("Parent node id is in incorrect format: {0}", parentNodeId));
            }
            
            return new UploadResponse();
        }

        #region Helper Methods

        private bool IsValidRequest(HttpContext context)
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && !context.Request.IsSecureConnection)
                throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");

            var username = context.Request["username"];
            var password = context.Request["password"];
            var ticket = context.Request["ticket"];

            var isValid = false;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var mp = MembershipProviderExtensions.GetUsersMembershipProvider();
                if (mp != null && mp.ValidateUser(username, password))
                {
                    var user = new User(username);
                    isValid = user.Applications.Any(app => app.alias == Constants.Applications.Media);

                    if (isValid)
                        AuthenticatedUser = user;
                }
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(ticket))
            {
                var t = FormsAuthentication.Decrypt(ticket);
                var user = new User(username);

                if (t != null)
                isValid = user.LoginName.ToLower() == t.Name.ToLower() && user.Applications.Any(app => app.alias == Constants.Applications.Media);

                if (isValid)
                    AuthenticatedUser = user;
            }
            else
            {
                var usr = User.GetCurrent();
                
                if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID) && usr != null)
                {
                    //The user is valid based on their cookies, but is the request valid? We need to validate
                    // against CSRF here. We'll do this by ensuring that the request contains a token which will
                    // be equal to the decrypted version of the current user's user context id.
                    var token = context.Request["__reqver"];
                    if (token.IsNullOrWhiteSpace() == false)
                    {
                        //try decrypting it
                        try
                        {
                            var decrypted = token.DecryptWithMachineKey();
                            //now check if it matches
                            if (decrypted == BasePage.umbracoUserContextID)
                            {
                                isValid = true;
                                AuthenticatedUser = usr;
                            }
                        }
                        catch
                        {
                           //couldn't decrypt, so it's invalid
                        }
                        
                    }
                }
            }

            return isValid;
        }

        private void CreateMediaTree(IEnumerable<Media> nodes, FolderListItem folder)
        {
            foreach (var media in nodes.Where(media => media != null && media.ContentType != null && media.ContentType.Alias == Constants.Conventions.MediaTypes.Folder))
            {
                var subFolder = new FolderListItem
                {
                    id = media.Id,
                    name = media.Text
                };

                if (media.HasChildren)
                {
                    CreateMediaTree(media.Children, subFolder);
                }

                folder.folders.Add(subFolder);
            }
        }

        private int GetMaxRequestLength()
        {
            var appSetting = Convert.ToInt32(ConfigurationManager.AppSettings["DesktopMediaUploaderMaxRequestLength"]);
            if (appSetting > 0)
                return appSetting;

            var configXml = new XmlDocument { PreserveWhitespace = true };
            configXml.Load(HttpContext.Current.Server.MapPath("/web.config"));

            var requestLimitsNode = configXml.SelectSingleNode("//configuration/system.webServer/security/requestFiltering/requestLimits");
            if (requestLimitsNode != null)
            {
                if (requestLimitsNode.Attributes != null && requestLimitsNode.Attributes["maxAllowedContentLength"] != null)
                {
                    var maxAllowedContentLength = Convert.ToInt32(requestLimitsNode.Attributes["maxAllowedContentLength"].Value);
                    if (maxAllowedContentLength > 0)
                        return maxAllowedContentLength;
                }
            }

            var httpRuntime = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;

            return httpRuntime == null ? 4096 : httpRuntime.MaxRequestLength;
        }

        private Media GetOrCreateFolder(Media parent, string name)
        {
            var children = parent.Id == -1 ? Media.GetRootMedias() : parent.Children;
            if (children.Length > 0)
            {
                foreach (var node in children.Where(node => node.Text.ToLower() == name.ToLower()))
                {
                    return node;
                }
            }

            var media = Media.MakeNew(name, MediaType.GetByAlias(Constants.Conventions.MediaTypes.Folder), User.GetUser(0), parent.Id);
            media.sortOrder = 0;
            media.Save();

            return media;
        }

        #endregion
    }

    public class MediaResponse
    {
        [XmlAttribute]
        public bool success { get; set; }
    }

    [XmlRoot("response")]
    public class ConfigResponse : MediaResponse
    {
        public string displayName { get; set; }
        public string umbracoPath { get; set; }
        public int maxRequestLength { get; set; }
    }

    [XmlRoot("response")]
    public class FolderListResponse : MediaResponse
    {
        public FolderListItem folder { get; set; }
    }

    [XmlType("folder")]
    public class FolderListItem
    {
        [XmlAttribute]
        public int id { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("folder")]
        public List<FolderListItem> folders { get; set; }

        public FolderListItem()
        {
            folders = new List<FolderListItem>();
        }
    }

    [XmlRoot("response")]
    public class UploadResponse : MediaResponse
    { }
}