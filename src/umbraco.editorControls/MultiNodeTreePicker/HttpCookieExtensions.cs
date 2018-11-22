using System;
using System.Linq;
using System.Web;

namespace umbraco.editorControls.MultiNodeTreePicker
{

    /// <summary>
    ///  A helper class to store and retrieve cookie values for the MNTP cookie.
    /// </summary>
    /// <remarks>
    /// The cookie is used to persist values from the client to the server since 
    /// it is much more complicated to try to persist these values between ajax request, 
    /// given the tree's current architecture.
    /// </remarks>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public static class HttpCookieExtensions
    {

        private enum CookieVals
        {
            /// <summary>
            /// XPath filter
            /// </summary>
            Xpf,
            /// <summary>
            /// XPath filter type
            /// </summary>
            Xpft,
            /// <summary>
            /// Start node
            /// </summary>
            Sn,
            /// <summary>
            /// Start node xpath expression type
            /// </summary>
            Snxet,
            /// <summary>
            /// Start node select type
            /// </summary>
            Snst,
            /// <summary>
            /// Start node xpath expression
            /// </summary>
            Snxe,
            /// <summary>
            /// Current editing node id
            /// </summary>
            Ceni
        }


        #region Setters

        internal static void MntpAddXPathFilter(this HttpCookie c, int id, string xpath)
        {
            c[string.Concat(CookieVals.Xpf, "_", id)] = xpath;
        }

        internal static void MntpAddXPathFilterType(this HttpCookie c, int id, XPathFilterType type)
        {
            c[string.Concat(CookieVals.Xpft, "_", id)] = ((int)type).ToString();
        }

        internal static void MntpAddStartNodeId(this HttpCookie c, int id, int startNodeId)
        {
            c[string.Concat(CookieVals.Sn, "_", id)] = startNodeId.ToString();
        }

        internal static void MntpAddStartNodeXPathExpressionType(this HttpCookie c, int id, XPathExpressionType xPathExpressionType)
        {
            c[string.Concat(CookieVals.Snxet, "_", id)] = ((int)xPathExpressionType).ToString();
        }

        internal static void MntpAddStartNodeSelectionType(this HttpCookie c, int id, NodeSelectionType startNodeSelectionType)
        {
            c[string.Concat(CookieVals.Snst, "_", id)] = ((int)startNodeSelectionType).ToString();
        }

        internal static void MntpAddStartNodeXPathExpression(this HttpCookie c, int id, string xPathExpression)
        {
            c[string.Concat(CookieVals.Snxe, "_", id)] = xPathExpression;
        }

        internal static void MntpAddCurrentEditingNode(this HttpCookie c, int id, int currEditingNodeId)
        {
            c[string.Concat(CookieVals.Ceni, "_", id)] = currEditingNodeId.ToString();
        }

        #endregion

        #region Getters

        internal static string MntpGetXPathFilter(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Xpf, dataTypeId)
                       ? c.Values[string.Concat(CookieVals.Xpf, "_", dataTypeId)]
                       : string.Empty;
        }

        internal static XPathFilterType MntpGetXPathFilterType(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Xpft, dataTypeId)
                       ? (XPathFilterType) Enum.ToObject(typeof (XPathFilterType),
                                                         int.Parse(
                                                             c.Values[string.Concat(CookieVals.Xpft, "_", dataTypeId)]))
                       : XPathFilterType.Disable;
        }

        internal static int MntpGetStartNodeId(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Sn, dataTypeId)
                       ? int.Parse(c.Values[string.Concat(CookieVals.Sn, "_", dataTypeId)])
                       : -1;
        }

        internal static XPathExpressionType MntpGetStartNodeXPathExpressionType(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Snxet, dataTypeId)
                       ? (XPathExpressionType)
                         Enum.ToObject(typeof(XPathExpressionType),
                                       int.Parse(c.Values[string.Concat(CookieVals.Snxet, "_", dataTypeId)]))
                       : XPathExpressionType.Global;
        }

        internal static NodeSelectionType MntpGetStartNodeSelectionType(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Snst, dataTypeId)
                       ? (NodeSelectionType)
                         Enum.ToObject(typeof(NodeSelectionType),
                                       int.Parse(c.Values[string.Concat(CookieVals.Snst, "_", dataTypeId)]))
                       : NodeSelectionType.Picker;
        }

        internal static string MntpGetStartNodeXPathExpression(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Snxe, dataTypeId)
                ? c.Values[string.Concat(CookieVals.Snxe, "_", dataTypeId)]
                : string.Empty;
        }

        internal static int MntpGetCurrentEditingNode(this HttpCookie c, int dataTypeId)
        {
            return c.ValidateCookieVal(CookieVals.Ceni, dataTypeId)
                       ? int.Parse(c.Values[string.Concat(CookieVals.Ceni, "_", dataTypeId)])
                       : -1;
        }

        private static bool ValidateCookieVal(this HttpCookie c, CookieVals val, int dataTypeId)
        {
            return dataTypeId == 0 ? false : (c.Values.Keys.Cast<string>().Where(x => x == string.Concat(val, "_", dataTypeId)).Any());
        }

        #endregion
    }
}
