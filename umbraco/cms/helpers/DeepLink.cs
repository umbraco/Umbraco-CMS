using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.BusinessLogic;
using System.Web;

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
            pathPaths.Reverse();
            for (int p = 0; p < pathPaths.Length; p++)
            {
                treePath.Add(string.Join("/", pathPaths.Take(p + 1).ToArray()));
            }
            string sPath = string.Join(",", treePath.ToArray());
            return sPath;
        }
        public static string Get(DeepLinkType type, string idOrFilePath)
        {
            string basePath = "/umbraco/umbraco.aspx";

            string section = string.Empty;
            string editorUrl = string.Empty;
            string idKey = string.Empty;

            switch (type)
            {
                case DeepLinkType.Content:
                    section = "content";
                    editorUrl = "editContent.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.Css:
                    section = "settings";
                    editorUrl = "settings/stylesheet/editStylesheet.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.DataType:
                    section = "developer";
                    editorUrl = "developer/datatypes/editDataType.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.DocumentType:
                    section = "settings";
                    editorUrl = "settings/editNodeTypeNew.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.Javascript:
                    section = "settings";
                    editorUrl = "settings/scripts/editScript.aspx";
                    idKey = "file";
                    break;
                case DeepLinkType.Macro:
                    section = "developer";
                    editorUrl = "developer/macros/editMacro.aspx";
                    idKey = "macroID";
                    break;
                case DeepLinkType.Media:
                    section = "media";
                    editorUrl = "editMedia.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.MediaType:
                    section = "settings";
                    editorUrl = "settings/editMediaType.aspx";
                    idKey = "id";
                    break;
                case DeepLinkType.RazorScript:
                    section = "developer";
                    editorUrl = "developer/python/editPython.aspx";
                    idKey = "file";
                    break;
                case DeepLinkType.Template:
                    section = "settings";
                    editorUrl = "settings/editTemplate.aspx";
                    idKey = "templateId";
                    break;
                case DeepLinkType.XSLT:
                    section = "developer";
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
                        return string.Format("{0}?app={1}&rightAction={2}#{1}", basePath, section, HttpContext.Current.Server.UrlEncode(rightAction));
                    }
                }
            }
            return null;
        }
    }
}
