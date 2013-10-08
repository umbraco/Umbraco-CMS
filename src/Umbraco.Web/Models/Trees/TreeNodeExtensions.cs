namespace Umbraco.Web.Models.Trees
{
    public static class TreeNodeExtensions
    {
        internal const string LegacyJsCallbackKey = "jsClickCallback";

        /// <summary>
        /// Legacy tree node's assign a JS method callback for when an item is clicked, this method facilitates that.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="jsCallback"></param>
        internal static void AssignLegacyJsCallback(this TreeNode treeNode, string jsCallback)
        {
            treeNode.AdditionalData[LegacyJsCallbackKey] = jsCallback;
        }
    }
}