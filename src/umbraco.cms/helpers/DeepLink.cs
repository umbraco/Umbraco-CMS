using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;
using System.Web;
using Umbraco.Core;

namespace umbraco.cms.helpers
{
    public class DeepLink
    {
        public static string GetTreePathFromFilePath(string filePath)
        {
            List<string> treePath = new List<string>();
            treePath.Add("-1");
            treePath.Add("init");
            string[] pathPaths = filePath.Split('/');
            
            for (int p = 0; p < pathPaths.Length; p++)
            {
                treePath.Add(string.Join("/", pathPaths.Take(p + 1).ToArray()));
            }
            string sPath = string.Join(",", treePath.ToArray());
            return sPath;
        }
        public static string GetAnchor(DeepLinkType type, string idOrFilePath, bool useJavascript)
        {
            string url = GetUrl(type, idOrFilePath, useJavascript);
            if (!string.IsNullOrEmpty(url))
            {
                if (!useJavascript)
                {
                    return string.Format("<a href=\"{0}\" target=\"_blank\">{1}&nbsp;&gt;</a>", url, ui.GetText("general", "edit"));
                }
                else
                {
                    return string.Format("<a href=\"{0}\">{1}&nbsp;&gt;</a>", url, ui.GetText("general", "edit"));
                }
            }
            return null;
        }
        public static string GetUrl(DeepLinkType type, string idOrFilePath, bool useJavascript)
        {
            string basePath = "/umbraco/umbraco.aspx";

            string section = string.Empty;
            string editorUrl = string.Empty;
            string idKey = string.Empty;

            switch (type)
            {
                case DeepLinkType.Content:
                    section = Constants.Applications.Content;
                    editorUrl = "editContent.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.Css:
                    section = Constants.Applications.Settings;
                    editorUrl = "settings/stylesheet/editStylesheet.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.DataType:
                    section = Constants.Applications.Developer;
                    editorUrl = "developer/datatypes/editDataType.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.DocumentType:
                    section = Constants.Applications.Settings;
                    editorUrl = "settings/editNodeTypeNew.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.Javascript:
                    section = Constants.Applications.Settings;
                    editorUrl = "settings/scripts/editScript.aspx";
                    idKey = "file";
                    break;
                case DeepLinkType.Macro:
                    section = Constants.Applications.Developer;
                    editorUrl = "developer/macros/editMacro.aspx";
                    idKey = "macroID";
                    break;
                case DeepLinkType.Media:
                    section = Constants.Applications.Media;
                    editorUrl = "editMedia.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.MediaType:
                    section = Constants.Applications.Settings;
                    editorUrl = "settings/editMediaType.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.RazorScript:
                    section = Constants.Applications.Developer;
                    editorUrl = "developer/python/editPython.aspx";
                    idKey = "file";
                    break;
                case DeepLinkType.Template:
                    section = Constants.Applications.Settings;
                    editorUrl = "settings/editTemplate.aspx";
                    idKey = "templateId";
                    break;
                case DeepLinkType.XSLT:
                    section = Constants.Applications.Developer;
                    editorUrl = "developer/xslt/editXslt.aspx";
                    idKey = "file";
                    break;
            }
            if (section != string.Empty)
            {
                User currentUser = User.GetCurrent();
                if (currentUser != null)
                {
                    //does the current user have access to this section
                    if (currentUser.Applications.Any(app => app.alias == section))
                    {
                        string rightAction = string.Format("{0}?{1}={2}", editorUrl, idKey, idOrFilePath);
                        if (!useJavascript)
                        {
                            string rightActionEncoded = HttpContext.Current.Server.UrlEncode(rightAction);
                            return string.Format("{0}?app={1}&rightAction={2}#{1}", basePath, section, rightActionEncoded);
                        }
                        else
                        {
                            return string.Format("javascript:UmbClientMgr.contentFrameAndSection('{0}','{1}');", section, rightAction);
                        }
                    }
                }
            }
            return null;
        }
    }
}
