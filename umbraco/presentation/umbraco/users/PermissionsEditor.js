/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference path="/umbraco_client/ui/jquery.js" />
/// <reference path="PermissionsHandler.asmx" />
/// <reference name="MicrosoftAjax.js"/>

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    $.fn.PermissionsEditor = function(opts) {
        return this.each(function() {
            var conf = $.extend({
                userId: -1,
                pPanelSelector: "",
                replacePChkBoxSelector: ""
            }, opts);
            new Umbraco.Controls.PermissionsEditor().init($(this), conf.userId, $(conf.pPanelSelector), conf.replacePChkBoxSelector);
        });
    };
    $.fn.PermissionsEditorAPI = function() {
        /// <summary>exposes the Permissions Editor API for the selected object</summary>
        return $(this).data("PermissionsEditor") == null ? null : $(this).data("PermissionsEditor");
    }
    Umbraco.Controls.PermissionsEditor = function() {
        /// <summary>
        /// A class to perform the AJAX callback methods for the PermissionsEditor
        /// </summary>
        return {
            //private members
            _userID: -1,
            _loadingContent: '<div align="center"><br/><br/><br/><br/><br/><br/><br/><img src="../../umbraco_client/images/progressBar.gif" /></div>',
            _pPanel: null,
            _tree: null,
            _selectedNodes: new Array(),
            _chkReplaceSelector: null,

            init: function(tree, uId, pPanel, chkReplaceSelector) {
                ///<summary>constructor function</summary>
                this._userID = parseInt(uId);
                this._pPanel = pPanel;
                this._tree = tree;
                this._chkReplaceSelector = chkReplaceSelector;

                //store the api object
                tree.data("PermissionsEditor", this);

                //bind the nodeClicked event
                var _this = this;
                var api = this._tree.UmbracoTreeAPI();
                api.addEventHandler("nodeClicked", function(e, n) { _this.treeNodeChecked(e, n) });
            },

            //public methods
            treeNodeChecked: function(e, data) {
                var vals = "";
                var nodeId = $(data).attr("id");
                this._tree.find("li a.checked").each(function() {
                    var cNodeId = $(this).parent("li").attr("id");
                    //if the check box is not the one thats just been checked, add it
                    if (cNodeId != nodeId && parseInt(cNodeId) > 0) {
                        vals += cNodeId + ",";
                    }
                });
                this._selectedNodes = vals.split(",");
                this._selectedNodes.pop(); //remove the last one as it will be empty
                //add the one that was just checked to the end of the array if
                if ($(data).children("a").hasClass("checked")) {
                    this._selectedNodes.push(nodeId);
                }
                if (this._selectedNodes.length > 0) {
                    this._beginShowNodePermissions(this._selectedNodes.join(","));
                    this._pPanel.show();
                }
                else {
                    this._pPanel.hide();
                }
            },
            setReplaceChild: function(doReplace) {
                alert(doReplace);
                this._replaceChildren = doReplace;
            },
            beginSavePermissions: function() {

                //ensure that there are nodes selected to save permissions against
                if (this._selectedNodes.length == 0) {
                    alert("No nodes have been selected");
                    return;
                }
                else if (!confirm("Permissions will be changed for nodes: " + this._selectedNodes.join(",") + ". Are you sure?")) {
                    return;
                }

                //get the list of checked permissions
                var checkedPermissions = "";
                this._pPanel.find("input:checked").each(function() {
                    checkedPermissions += $(this).val();
                });

                var replaceChildren = $(this._chkReplaceSelector).is(":checked");

                this._setUpdateProgress();
                var _this = this;
                setTimeout(function() { _this._savePermissions(_this._selectedNodes.join(","), checkedPermissions, replaceChildren); }, 10);
            },

            //private methods
            _addItemToSelectedCollection: function(chkbox) {
                if (chkbox.checked)
                    this._selectedNodes.push(chkbox.value);
                else {
                    var joined = this._selectedNodes.join(',');
                    joined = joined.replace(chkbox.value, '');
                    this._selectedNodes = joined.split(',');
                    this._selectedNodes = ContentTreeControl_ReBuildArray(contentTreeControl_selected);
                }

            },
            _beginShowNodePermissions: function(selectedIDs) {
                this._setUpdateProgress();
                var _this = this;
                setTimeout(function() { _this._showNodePermissions(selectedIDs); }, 10);
                //setTimeout("PermissionsEditor._showNodePermissions('" + selectedIDs + "');", 10);
            },
            _showNodePermissions: function(selectedIDs) {
                var _this = this;
                umbraco.cms.presentation.user.PermissionsHandler.GetNodePermissions(this._userID, selectedIDs, function(r) { _this._showNodePermissionsCallback(r); });
            },
            _showNodePermissionsCallback: function(result) {
                this._pPanel.html(result);
            },
            _savePermissions: function(nodeIDs, selectedPermissions, replaceChildren) {
                var _this = this;
                umbraco.cms.presentation.user.PermissionsHandler.SaveNodePermissions(this._userID, nodeIDs, selectedPermissions, replaceChildren, function(r) { _this._savePermissionsHandler(r) });
            },
            _savePermissionsHandler: function(result) {
                if (UmbClientMgr.mainWindow().UmbSpeechBubble != null)
                    UmbClientMgr.mainWindow().UmbSpeechBubble.ShowMessage("save", "Saved", "Permissions Saved");
                this._pPanel.html(result);
            },
            _setUpdateProgress: function() {
                this._pPanel.html(this._loadingContent);
            }
        }
    }
})(jQuery);