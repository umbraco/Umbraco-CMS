using System;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using Umbraco.Core.IO;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.BasePages;

namespace umbraco.presentation.create
{
    /// <summary>
    /// Summary description for dialogHandler_temp.
    /// </summary>
    [Obsolete("This class is no longer used, it has been replaced by Umbraco.Web.UI.LegacyDialogHandler which will also eventually be deprecated")]
    public class dialogHandler_temp
    {
        public static void Delete(string NodeType, int NodeId)
        {
            Delete(NodeType, NodeId, "");
        }
        public static void Delete(string NodeType, int NodeId, string Text)
        {
            LegacyDialogHandler.Delete(
                new HttpContextWrapper(HttpContext.Current),
                BasePage.Current.getUser(),
                NodeType, NodeId, Text);
        }

        public static string Create(string NodeType, int NodeId, string Text)
        {
            return Create(NodeType, 0, NodeId, Text);
        }

        public static string Create(string NodeType, int TypeId, int NodeId, string Text)
        {
            return LegacyDialogHandler.Create(
                new HttpContextWrapper(HttpContext.Current),
                BasePage.Current.getUser(),
                NodeType, NodeId, Text, TypeId);
        }
    }
}
