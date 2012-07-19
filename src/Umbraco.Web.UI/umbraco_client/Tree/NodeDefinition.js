/// <reference name="MicrosoftAjax.js"/>
/// <reference path="/umbraco_client/Application/NamespaceManager.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

Umbraco.Controls.NodeDefinition = function() {
    /// <summary>
    /// An object that defines the details of a current node selected in the tree.
    /// Contains the logic to modify the html markup of a node such as styles.
    /// </summary>
    return {
        jsTree: null,
        jsNode: null,
        nodeId: null,
        nodeName: null,
        nodeType: null,
        sourceUrl: null,
        menu: null,
        treeType: null,
        _isDebug: false,
        
        updateDefinition: function(jsTree, jsNode, nodeId, nodeName, nodeType, sourceUrl, menu, treeType) {
            /// <summary>
            /// Updates the current objects properties at one time
            /// </summary>
            /// <param name="jsTree">A reference to the current node's jsTree</param>
            /// <param name="jsNode">A reference to the jquery li node</param>
            /// <param name="nodeId">The node id of the node</param>
            /// <param name="nodeName">The node name of the node</param>
            /// <param name="nodeType">The node type/tree type of the node</param>

            this.jsTree = jsTree;
            this.jsNode = jsNode;
            this.nodeId = nodeId;
            this.nodeName = nodeName;
            this.nodeType = nodeType;
            this.sourceUrl = sourceUrl;
            this.menu = menu;
            this.treeType = treeType;

            this._debug("updateDefinition: " + nodeId + ", " + nodeName + ", " + nodeType);
        },

        _debug: function(strMsg) {
            if (this._isDebug) {
                Sys.Debug.trace("NodeDefinition: " + strMsg);
            }
        }
    }
}