using System;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation.Trees;

namespace umbraco.editorControls.MultiNodeTreePicker
{
	/// <summary>
	/// BaseTree extensions for MultiNodeTreePicker.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public static class BaseTreeExtensions
    {

        internal const int NoAccessId = -123456789;
	    internal const int NoChildNodesId = -897987654;

        /// <summary>
        /// Determines if it needs to render a null tree based on the start node id and returns true if it is the case. 
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="startNodeId"></param>
        /// <param name="rootNode"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        internal static bool SetNullTreeRootNode(this BaseTree tree, int startNodeId, ref XmlTreeNode rootNode, string app)
        {
            if (startNodeId == NoAccessId)
            {
                rootNode = new NullTree(app).RootNode;
                rootNode.Text = "You do not have permission to view this tree";
                rootNode.HasChildren = false;
                rootNode.Source = string.Empty;
                return true;
            }

            if (startNodeId == NoChildNodesId)
            {
                rootNode = new NullTree(app).RootNode;
                rootNode.Text = "[No start node found]";
                rootNode.HasChildren = false;
                rootNode.Source = string.Empty;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to determine the start node id while taking into account a user's security
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="definedStartNode"></param>
        /// <param name="userStartNode"></param>
        /// <returns></returns>
        internal static int GetStartNodeId(this BaseTree tree, Content definedStartNode, Content userStartNode)
        {
            if (userStartNode == null)
            {
                throw new ArgumentNullException("userStartNode");
            }

            //the output start node id
            var determinedStartNodeId = uQuery.RootNodeId;

            if (definedStartNode == null)
            {
                //if the defined (desired) start node is null (could not be found), return NoChildNodesId
                determinedStartNodeId = NoChildNodesId;
            }
			else if (definedStartNode.Id == uQuery.RootNodeId)
            {
                //if the id is -1, then the start node is the user's start node
                determinedStartNodeId = userStartNode.Id;
            }
			else if (definedStartNode.Path.Split(',').Contains(userStartNode.Id.ToString()))
            {
                //If User's start node id is found in the defined path, then the start node id
                //can (allowed) be the defined path.
                //This should always work for administrator (-1)

                determinedStartNodeId = definedStartNode.Id;
            }
			else if (userStartNode.Path.Split(',').Contains(definedStartNode.Id.ToString()))
            {
                //if the defined start node id is found in the user's path, then the start node id
                //can only be the user's, not the actual start
                determinedStartNodeId = userStartNode.Id;
            }
			else if (!definedStartNode.Path.Split(',').Contains(userStartNode.Id.ToString()))
            {
                //they should not have any access!
                determinedStartNodeId = NoAccessId;
            }

            return determinedStartNodeId;
        }

        /// <summary>
        /// Returns the data type id for the current base tree
        /// </summary>
        /// <remarks>
        /// The data type definition id is persisted between request as a query string.
        /// This is used to retrieve values from the cookie which are easier persisted values
        /// than trying to append everything to custom query strings.
        /// </remarks>
        /// <param name="tree"></param>
        /// <returns></returns>
        internal static int GetDataTypeId(this BaseTree tree)
        {
            var id = -1;
            int.TryParse(tree.NodeKey, out id);
            return id;
        }

        /// <summary>
        /// return the xpath statement stored in the cookie for this control id
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        internal static string GetXPathFromCookie(this BaseTree tree, int dataTypeDefId)
        {
            //try to read an existing cookie
            var cookie = HttpContext.Current.Request.Cookies["MultiNodeTreePicker"];
            if (cookie != null && cookie.Values.Count > 0)
            {
                return cookie.MntpGetXPathFilter(dataTypeDefId);
            }
            return "";
        }


        /// <summary>
        /// Returns the xpath filter from the cookie for the current data type
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="dataTypeDefId"></param>
        /// <returns></returns>
        internal static XPathFilterType GetXPathFilterTypeFromCookie(this BaseTree tree, int dataTypeDefId)
        {
            //try to read an existing cookie
            var cookie = HttpContext.Current.Request.Cookies["MultiNodeTreePicker"];
            if (cookie != null && cookie.Values.Count > 0)
            {
                return cookie.MntpGetXPathFilterType(dataTypeDefId);
            }
            return XPathFilterType.Disable;
        }        

        /// <summary>
        /// Helper method to return the persisted cookied value for the tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tree"></param>
        /// <param name="output"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        internal static T GetPersistedCookieValue<T>(this BaseTree tree, Func<HttpCookie, T> output, T defaultVal)
        {
            var cookie = HttpContext.Current.Request.Cookies["MultiNodeTreePicker"];
            if (cookie != null && cookie.Values.Count > 0)
            {
                return output(cookie);
            }
            return defaultVal;
        }

        /// <summary>
        /// This will return the normal service url based on id but will also ensure that the data type definition id is passed through as the nodeKey param
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="id">The id.</param>
        /// <param name="dataTypeDefId">The data type def id.</param>
        /// <returns></returns>
        /// <remarks>
        /// We only need to set the custom source to pass in our extra NodeKey data.
        /// By default the system will use one or the other: Id or NodeKey, in this case
        /// we are sort of 'tricking' the system and we require both.
        /// Umbraco allows you to theoretically pass in any source as long as it meets the standard
        /// which means you can pass around any arbitrary data to your trees in the form of a query string,
        /// though it's just a bit convoluted to do so.
        /// </remarks>
        internal static string GetTreeServiceUrlWithParams(this BaseTree tree, int id, int dataTypeDefId)
        {
            var url = tree.GetTreeServiceUrl(id);
            //append the node key
            return url + "&nodeKey=" + dataTypeDefId;
        }

       

    }
}
