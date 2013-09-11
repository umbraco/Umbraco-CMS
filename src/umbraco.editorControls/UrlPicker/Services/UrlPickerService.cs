using System;
using System.IO;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Linq;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.editorControls.UrlPicker.Services
{
    /// <summary>
    /// Client side ajax utlities for the URL Picker
    /// </summary>
    [ScriptService]
    [WebService]
    public class UrlPickerService : WebService
    {
        /// <summary>
        /// Returns the NiceUrl of a content node
        /// </summary>
        /// <param name="id">The content node's ID</param>
        /// <returns>The NiceUrl of that content node</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ContentNodeUrl(int id)
        {
            Authorize();

            return library.NiceUrl(id);
        }

        /// <summary>
        /// Returns the URL of the default file in a media node (i.e. the "umbracoFile" property)
        /// else cycles through media node properties looking for "/media/" URLs
        /// </summary>
        /// <param name="id">The media node's ID</param>
        /// <returns>The URL of the file</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string MediaNodeUrl(int id)
        {
            Authorize();

            var media = XElement.Parse(library.GetMedia((int)id, false).Current.InnerXml);

            var umbracoFile = media.Element(Constants.Conventions.Media.File);

            if (umbracoFile != null)
            {
                return umbracoFile.Value;
            }
            else
            {
                // Cycle through other properties
                foreach (var element in media.Elements())
                {
                    // Check if the property looks like a URL to the media folder
                    if (element != null &&
                        !string.IsNullOrEmpty(element.Value) &&
                        element.Value.Contains("/media/"))
                    {
                        return element.Value;
                    }
                }
            }

            return null;
        }

        internal static void Authorize()
        {
            if (!umbraco.BasePages.BasePage.ValidateUserContextID(umbraco.BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");

        }

        /// <summary>
        /// File locker
        /// </summary>
        private static readonly object m_Locker = new object();

        /// <summary>
        /// Ensures that the web service is available on the file system of the
        /// web service
        /// </summary>
        internal static void Ensure()
        {
            var serviceFile = Path.Combine(IOHelper.MapPath(SystemDirectories.Umbraco), "plugins/UrlPicker/UrlPickerService.asmx");

            if (!File.Exists(serviceFile))
            {
                lock (m_Locker)
                {
                    // double check locking
                    if (!File.Exists(serviceFile))
                    {
                        // now create our new local web service
                        var wServiceFile = new FileInfo(serviceFile);
                        if (!wServiceFile.Exists)
                        {
                            var wServiceTxt = UrlPickerServiceResource.UrlPickerService;

                            if (!wServiceFile.Directory.Exists)
                            {
                                wServiceFile.Directory.Create();
                            }

                            using (var sw = new StreamWriter(wServiceFile.Create()))
                            {
                                sw.Write(wServiceTxt);
                            }
                        }
                    }
                }
            }
        }
    }
}