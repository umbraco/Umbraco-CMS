using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Xml;
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
            context.Response.Clear();
            context.Response.ContentType = @"text\xml";
            context.Response.Charset = "UTF-8";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetAllowResponseInBrowserHistory(true);

            var xmlTextWriter = new XmlTextWriter(context.Response.OutputStream, Encoding.UTF8);
            xmlTextWriter.WriteStartDocument();
            xmlTextWriter.WriteStartElement("response");

            string action = context.Request["action"];

            if (IsValidRequest(context, xmlTextWriter) && !string.IsNullOrEmpty(action))
            {
                switch (action.ToLower())
                {
                    case "config":
                        ProcessConfigRequest(context, xmlTextWriter);
                        break;
                    case "folderlist":
                        ProcessFolderListRequest(context, xmlTextWriter);
                        break;
                    case "upload":
                        ProcessUploadRequest(context, xmlTextWriter);
                        break;
                }
            }

            xmlTextWriter.WriteEndElement();
            xmlTextWriter.WriteEndDocument();
            xmlTextWriter.Flush();
            xmlTextWriter.Close();

            context.Response.End();
        }

        public void ProcessConfigRequest(HttpContext context, XmlTextWriter xmlTextWriter)
        {
            xmlTextWriter.WriteElementString("displayName", new User(context.Request["username"]).Name);
            xmlTextWriter.WriteElementString("umbracoPath", VirtualPathUtility.ToAbsolute(GlobalSettings.Path));
            xmlTextWriter.WriteElementString("maxRequestLength", GetMaxRequestLength().ToString());
        }

        public void ProcessFolderListRequest(HttpContext context, XmlTextWriter xmlTextWriter)
        {
            xmlTextWriter.WriteStartElement("folder");

            var startMediaId = AuthenticatedUser.StartMediaId;
            if (startMediaId < 1)
            {
                xmlTextWriter.WriteAttributeString("id", "-1");
                xmlTextWriter.WriteAttributeString("name", "Media");

                CreateMediaTree(Media.GetRootMedias(), xmlTextWriter);
            }
            else
            {
                var root = new Media(startMediaId);

                xmlTextWriter.WriteAttributeString("id", root.Id.ToString());
                xmlTextWriter.WriteAttributeString("name", root.Text);

                CreateMediaTree(root.Children, xmlTextWriter);
            }

            xmlTextWriter.WriteEndElement();
        }

        public void ProcessUploadRequest(HttpContext context, XmlTextWriter xmlTextWriter)
        {
            int parentNodeId;
            if (int.TryParse(context.Request["parentNodeId"], out parentNodeId) && context.Request.Files.Count > 0)
            {
                try
                {
                    // Check Path
                    if (!string.IsNullOrEmpty(context.Request["path"]))
                    {
                        var pathParts = context.Request["path"].Trim('/').Split('/');

                        var parentNode = new Media(parentNodeId);
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
                    bool replaceExisting = (context.Request["replaceExisting"] == "1");

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
                                ContentType = uploadFile.ContentType,
                                ContentLength = uploadFile.ContentLength,
                                InputStream = uploadFile.InputStream
                            };

                            // Get concrete MediaFactory
                            var factory = MediaFactory.GetMediaFactory(parentNodeId, postedMediaFile, AuthenticatedUser);

                            // Handle media Item
                            var media = factory.HandleMedia(parentNodeId, postedMediaFile, AuthenticatedUser, replaceExisting);
                        }
                    }

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
        }

        #region Helper Methods

        private bool IsValidRequest(HttpContext context, XmlTextWriter xmlTextWriter)
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

            xmlTextWriter.WriteAttributeString("success", isValid.ToString().ToLower());

            return isValid;
        }

        private void CreateMediaTree(IEnumerable<Media> nodes, XmlWriter xmlTextWriter)
        {
            foreach (var media in nodes.Where(media => media != null && media.ContentType != null && media.ContentType.Alias == "Folder"))
            {
                xmlTextWriter.WriteStartElement("folder");
                xmlTextWriter.WriteAttributeString("id", media.Id.ToString());
                xmlTextWriter.WriteAttributeString("name", media.Text);

                if (media.HasChildren)
                {
                    CreateMediaTree(media.Children, xmlTextWriter);
                }

                xmlTextWriter.WriteEndElement();
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
}