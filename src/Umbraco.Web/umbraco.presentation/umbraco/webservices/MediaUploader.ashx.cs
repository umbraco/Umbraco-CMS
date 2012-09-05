using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.media;

namespace umbraco.presentation.umbraco.webservices
{
    public class MediaUploader : IHttpHandler
    {
        protected User AuthenticatedUser { get; set; }

        public bool IsReusable
        {
            get { return true; }
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
                    context.Response.ContentType = @"application/json";

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
                        
                        foreach (var pathPart in pathParts)
                        {
                            if (!string.IsNullOrEmpty(pathPart))
                            {
                                parentNode = GetOrCreateFolder(parentNode, pathPart);
                            }
                        }
                        parentNodeId = parentNode.Id;
                    }

                    // Check whether to replace existing
                    var parsed = false;
                    bool replaceExisting = (context.Request["replaceExisting"] == "1" || (bool.TryParse(context.Request["replaceExisting"], out parsed) && parsed));

                    // loop through uploaded files
                    for (var j = 0; j < context.Request.Files.Count; j++)
                    {
                        // get the current file
                        var uploadFile = context.Request.Files[j];

                        // if there was a file uploded
                        if (uploadFile.ContentLength > 0)
                        {
                            var postedMediaFile = new PostedMediaFile
                            {
                                FileName = uploadFile.FileName,
                                DisplayName = context.Request["name"],
                                ContentType = uploadFile.ContentType,
                                ContentLength = uploadFile.ContentLength,
                                InputStream = uploadFile.InputStream,
                                ReplaceExisting = replaceExisting
                            };

                            // Get concrete MediaFactory
                            var factory = MediaFactory.GetMediaFactory(parentNodeId, postedMediaFile, AuthenticatedUser);

                            // Handle media Item
                            var media = factory.HandleMedia(parentNodeId, postedMediaFile, AuthenticatedUser);
                        }
                    }

                    var scripts = new ClientTools(new Page());
                    scripts.SyncTree(parentNode.Path, true);

                    // log succes
                    Log.Add(LogTypes.New, parentNodeId, "Succes");
                }
                catch (Exception e)
                {
                    // log error
                    Log.Add(LogTypes.Error, parentNodeId, e.ToString());
                }
            }
            else
            {
                // log error
                Log.Add(LogTypes.Error, -1, "Parent node id is in incorrect format");
            }
            
            
            
            return new UploadResponse();
        }

        #region Helper Methods

        private bool IsValidRequest(HttpContext context)
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && !context.Request.IsSecureConnection)
                throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");

            string username = context.Request["username"];
            string password = context.Request["password"];
            string ticket = context.Request["ticket"];

            bool isValid = false;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var mp = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider];
                if (mp.ValidateUser(username, password))
                {
                    var user = new User(username);
                    isValid = user.Applications.Any(app => app.alias == "media");

                    if (isValid)
                        AuthenticatedUser = user;
                }
            }
            else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(ticket))
            {
                var t = FormsAuthentication.Decrypt(ticket);
                var user = new User(username);
                isValid = user.LoginName.ToLower() == t.Name.ToLower() && user.Applications.Any(app => app.alias == "media");

                if (isValid)
                    AuthenticatedUser = user;
            }
            else if (User.GetCurrent() != null)
            {
                isValid = true;
                AuthenticatedUser = User.GetCurrent();
            }

            return isValid;
        }

        private void CreateMediaTree(IEnumerable<Media> nodes, FolderListItem folder)
        {
            foreach (var media in nodes.Where(media => media != null && media.ContentType != null && media.ContentType.Alias == "Folder"))
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

            var configXml = new XmlDocument();
            configXml.PreserveWhitespace = true;
            configXml.Load(HttpContext.Current.Server.MapPath("/web.config"));

            var requestLimitsNode = configXml.SelectSingleNode("//configuration/system.webServer/security/requestFiltering/requestLimits");
            if (requestLimitsNode != null)
            {
                if (requestLimitsNode.Attributes["maxAllowedContentLength"] != null)
                {
                    var maxAllowedContentLength = Convert.ToInt32(requestLimitsNode.Attributes["maxAllowedContentLength"].Value);
                    if (maxAllowedContentLength > 0)
                        return maxAllowedContentLength;
                }
            }

            var httpRuntime = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            if (httpRuntime != null)
                return httpRuntime.MaxRequestLength;

            return 4096;
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

            var media = Media.MakeNew(name, MediaType.GetByAlias("Folder"), User.GetUser(0), parent.Id);
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