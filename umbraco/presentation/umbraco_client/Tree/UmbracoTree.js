/// <reference path="/umbraco_client/Application/UmbracoUtils.js" />
/// <reference path="/umbraco_client/ui/jquery.js" />
/// <reference path="/umbraco_client/ui/jqueryui.js" />
/// <reference path="tree_component.js" />
/// <reference path="/umbraco_client/Application/UmbracoApplicationActions.js" />
/// <reference path="NodeDefinition.js" />
/// <reference name="MicrosoftAjax.js"/>

//TODO: SD: Do we need to do this for opera??
//operaContextMenu: function(oItem) { if (window.opera) { return showContextMenu(oItem); } else { this.toggle(oItem); } },

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {
    $.fn.UmbracoTree = function(opts) {
        return this.each(function() {

            var conf = $.extend({
                jsonFullMenu: null,
                jsonInitNode: null,
                appActions: null,
                uiKeys: null,
                app: "",
                showContext: true,
                isDialog: false,
                treeType: "standard"
            }, opts);
            new Umbraco.Controls.UmbracoTree().init($(this), conf);
        });
    };
    $.fn.UmbracoTreeAPI = function() {
        /// <summary>exposes the Umbraco Tree api for the selected object</summary>
        return $(this).data("UmbracoTree") == null ? null : $(this).data("UmbracoTree");
    };

    Umbraco.Controls.UmbracoTree = function() {
        /// <summary>
        /// The object that manages the Umbraco tree.
        /// Has these events: syncNotFound, syncFound, rebuiltTree, newchildNodeFound, nodeMoved, nodeCopied, ajaxError, nodeClicked
        /// </summary>
        return {
            _actionNode: new Umbraco.Controls.NodeDefinition(), //the most recent node right clicked for context menu
            _activeTreeType: "content", //tracks which is the active tree type, this is used in searching and syncing.
            _recycleBinId: -20,
            _fullMenu: null,
            _initNode: null,
            _menuActions: null,
            _tree: null, //reference to the jsTree object
            _uiKeys: null,
            _container: null,
            _app: null, //the reference to the current app
            _showContext: true,
            _isDialog: false,
            _isDebug: true, //set to true to enable alert debugging
            _loadedApps: [], //stores the application names that have been loaded to track which JavaScript code has been inserted into the DOM
            _serviceUrl: "", //a path to the tree client service url
            _dataUrl: "", //a path to the tree data service url
            _treeType: "standard", //determines the type of tree: 'standard', 'checkbox' = checkboxes enabled, 'inheritedcheckbox' = parent nodes have checks inherited from children
            _treeClass: "umbTree", //used for other libraries to detect which elements are an umbraco tree

            addEventHandler: function(fnName, fn) {
                /// <summary>Adds an event listener to the event name event</summary>
                $(this).bind(fnName, fn);
            },

            removeEventHandler: function(fnName, fn) {
                /// <summary>Removes an event listener to the event name event</summary>
                $(this).unbind(fnName, fn);
            },

            init: function(jItem, opts) {
                /// <summary>Initializes the tree with the options and stores the tree API in the jQuery data object for the current element</summary>
                this._init(opts.jsonFullMenu, opts.jsonInitNode, jItem, opts.appActions, opts.uiKeys, opts.app, opts.showContext, opts.isDialog, opts.treeType, opts.serviceUrl, opts.dataUrl);
                //store the api
                jItem.addClass(this._treeClass);
                jItem.data("UmbracoTree", this);
            },

            rebuildTree: function(app) {
                /// <summary>This will rebuild the tree structure for the application specified</summary>

                this._debug("rebuildTree");

                if (this._app == null || (this._app.toLowerCase() == app.toLowerCase())) {
                    this._debug("not rebuilding");
                    return;
                }
                else {
                    this._app = app;
                }

                //remove the old menu from the DOM as jsTree will keep appending new ones.
                $("div").remove(".tree-default-context");
                //kill the tree
                this._tree.destroy();
                this._container.hide();
                var _this = this;
                //need to get the init node for the new app
                var parameters = "{'app':'" + app + "','showContextMenu':'" + this._showContext + "', 'isDialog':'" + this._isDialog + "'}"
                $.ajax({
                    type: "POST",
                    url: this._serviceUrl,
                    data: parameters,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function(msg) {
                        //recreates the tree
                        if ($.inArray(msg.app, _this._loadedApps) == -1) {
                            _this._debug("loading js for app: " + msg.app);
                            _this._loadedApps.push(msg.app);
                            //inject the scripts
                            _this._container.after("<script>" + msg.js + "</script>");
                        }
                        _this._initNode = eval(msg.json);
                        _this._tree = $.tree_create();
                        _this._tree.init(_this._container, _this._getInitOptions());
                        _this._container.show();
                        _this._loadChildNodes(_this._container.find("li:first"), null);
                        $(_this).trigger("rebuiltTree", [msg.app]);
                    },
                    error: function(e) {
                        _this._debug("rebuildTree: AJAX error occurred");
                        $(_this).trigger("ajaxError", [{ msg: "rebuildTree"}]);
                    }
                });

            },

            syncTree: function(path, forceReload) {
                /// <summary>
                /// Syncronizes the tree with the path supplied and makes that node visible/selected.
                /// </summary>
                /// <param name="path">The path of the node</param>
                /// <param name="forceReload">If true, will ensure that the node to be synced is synced with data from the server</param>

                this._debug("syncTree: " + path + ", " + forceReload);

                this._syncTree.call(this, path, forceReload, null, null);

            },

            childNodeCreated: function() {
                /// <summary>
                /// Reloads the children of the current action node and selects the node that didn't exist there before.
                /// If it cannot determine which node is new, then no node is selected.	If the children are not already
                /// loaded, then it is impossible for this method to determine which child is new.	
                /// </summary>

                this._debug("childNodeCreated");

                //store the current child ids so we can determine which one is the new one
                var childrenIds = new Array();
                this._actionNode.jsNode.find("ul > li").each(function() {
                    childrenIds.push($(this).attr("id"));
                });
                var _this = this;
                var currId = this._actionNode.nodeId;
                this.reloadActionNode(true, false, function(success) {
                    if (success && childrenIds.length > 0) {
                        var found = false;
                        var actionNode = _this.findNode(currId);
                        if (actionNode) {
                            actionNode.find("ul > li").each(function() {
                                //if the id of the current child is not found in the original list, then this is the new one, store it
                                if ($.inArray($(this).attr("id"), childrenIds) == -1) {
                                    found = $(this);
                                }
                            });
                        }
                        if (found) {
                            _this._debug("childNodeCreated: selecting new child node: " + found.attr("id"));
                            _this.selectNode(found, true, true);
                            $(_this).trigger("newChildNodeFound", [found]);
                            return;
                        }
                    }
                    _this._debug("childNodeCreated: could not select new child!");
                });
            },

            moveNode: function(nodeId, parentPath) {
                /// <summary>Moves a node in the tree. This will remove the existing node by id and sync the tree to the new path</summary>

                this._debug("moveNode");

                //remove the old node
                var old = this.findNode(nodeId);
                if (old) old.remove();

                //build the path to the new node
                var newPath = parentPath + "," + nodeId;
                //create an event handler for the tree sync
                var _this = this;
                var foundHandler = function(EV, node) {
                    //remove the event handler from firing again
                    _this.removeEventHandler("syncFound", foundHandler);
                    //ensure the node is selected, ensure the event is fired and reselect is true since jsTree thinks this node is already selected by id
                    _this.selectNode(node, false, true);
                    $(_this).trigger("nodeMoved", [node]);
                };
                //add the event handler for the tree sync and sync the tree
                this.addEventHandler("syncFound", foundHandler);
                this.syncTree(newPath);
            },

            copyNode: function(nodeId, parentPath) {
                /// <summary>Copies a node in the tree. This will keep the current node selected but will sync the tree to show the copied node too</summary>

                this._debug("copyNode");

                var originalNode = this.findNode(nodeId);

                //create an event handler for the tree sync
                var _this = this;
                var foundHandler = function(EV, node) {
                    //remove the event handler from firing again
                    _this.removeEventHandler("syncFound", foundHandler);
                    //now that the new parent node is found, expand it
                    _this._loadChildNodes(node, null);
                    //reselect the original node since sync will select the one that was copied
                    if (originalNode) _this.selectNode(originalNode, true);
                    $(_this).trigger("nodeCopied", [node]);
                };
                //add the event handler for the tree sync and sync the to the parent path
                this.addEventHandler("syncFound", foundHandler);
                this.syncTree(parentPath);
            },

            findNode: function(nodeId, findGlobal) {
                /// <summary>Returns either the found branch or false if not found in the tree</summary>
                /// <param name="findGlobal">Optional. If true, disregards the tree type and searches the entire tree for the id</param>
                var _this = this;
                var branch = this._container.find("li[id='" + nodeId + "']");
                if (!findGlobal) branch = branch.filter(function() {
                    return ($(this).attr("umb:type") == _this._activeTreeType); //filter based on custom namespace requires custom function
                });
                var found = branch.length > 0 ? branch : false;
                this._debug("findNode: " + nodeId + " in '" + this._activeTreeType + "' tree. Found? " + found.length);
                return found;
            },

            selectNode: function(node, supressEvent, reselect) {
                /// <summary>Makes the selected node the active node, but only if it is not already selected or if reselect is true</summary>
                /// <param name="supressEvent">If set to true, will select the node but will supress the onSelected event</param>
                /// <param name="reselect">If set to true, will call the select_branch method even if the node is already selected</param>

                this._debug("selectNode");

                var selectedId = this._tree.selected != null ? $(this._tree.selected[0]).attr("id") : null;
                if (reselect || (selectedId == null || selectedId != node.attr("id"))) {
                    //if we don't wan the event to fire, we'll set the callback to a null method and set it back after we call the select_branch method
                    if (supressEvent) {
                        this._tree.settings.callback.onselect = function() { };
                    }
                    this._tree.select_branch(node);
                    //reset the method / maintain scope in callback
                    var _this = this;
                    this._tree.settings.callback.onselect = function(N, T) { _this.onSelect(N, T) };
                }
            },

            reloadActionNode: function(supressSelect, supressChildReload, callback) {
                /// <summary>
                /// Gets the current action node's parent's data source url, then passes this url and the current action node's id
                /// to a web service. The webservice will find the JSON data for the current action node and return it. This
                /// will parse the returned JSON into html and replace the current action nodes' markup with the refreshed server data.
                /// If by chance, the ajax call fails because of inconsistent data (a developer has implemented poor tree design), then
                /// this use the build in jsTree reload which works ok.
                /// </summary>
                /// <param name="callback">
                /// A callback function which will have a boolean parameter passed. True = the reload was succesful,
                /// False = the reload failed and the generic _tree.refresh() method was used.
                /// </param>
                this._debug("reloadActionNode: supressSelect = " + supressSelect + ", supressChildReload = " + supressChildReload);

                if (this._actionNode != null && this._actionNode.jsNode != null) {
                    var nodeParent = this._actionNode.jsNode.parents("li:first");
                    this._debug("reloadActionNode: found " + nodeParent.length + " parent nodes");
                    if (nodeParent.length == 1) {
                        var nodeDef = this._getNodeDef(nodeParent);
                        this._debug("reloadActionNode: loading ajax for node: " + nodeDef.nodeId);
                        var _this = this;
                        //replace the node to refresh with loading and return the new loading element
                        var toReplace = $("<li class='last'><a class='loading' href='#'>" + (this._tree.settings.lang.loading || "Loading ...") + "</a></li>").replaceAll(this._actionNode.jsNode);
                        $.get(this._getUrl(nodeDef.sourceUrl), null,
                            function(msg) {
                                if (!msg || msg.length == 0) {
                                    _this._debug("reloadActionNode: error loading ajax data, performing jsTree refresh");
                                    _this._tree.refresh(); /*try jsTree refresh as last resort */
                                    if (callback != null) callback.call(_this, false);
                                    return;
                                }
                                //filter the results to find the object corresponding to the one we want refreshed
                                var oFound = null;
                                for (var o in msg) {
                                    if (msg[o].attributes != null && msg[o].attributes.id == _this._actionNode.nodeId) {
                                        oFound = _this._tree.parseJSON(msg[o]);
                                        //ensure the tree type is the same too
                                        if ($(oFound).attr("umb:type") == _this._actionNode.treeType) { break; }
                                        else { oFound = null; }
                                    }
                                }
                                if (oFound != null) {
                                    _this._debug("reloadActionNode: node is refreshed!");
                                    var reloaded = $(oFound).replaceAll(toReplace);
                                    _this._configureNodes(reloaded, true);
                                    if (!supressSelect) _this.selectNode(reloaded);
                                    if (!supressChildReload) {
                                        _this._loadChildNodes(reloaded, function() {
                                            if (callback != null) callback.call(_this, true);
                                        });
                                    }
                                    else { if (callback != null) callback.call(_this, true); }
                                }
                                else {
                                    _this._debug("reloadActionNode: error finding child node in ajax data, performing jsTree refresh");
                                    _this._tree.refresh(); /*try jsTree refresh as last resort */
                                    if (callback != null) callback.call(_this, false);
                                }
                            }, "json");
                        return;
                    }

                    this._debug("reloadActionNode: error finding parent node, performing jsTree refresh");
                    this._tree.refresh(); /*try jsTree refresh as last resort */
                    if (callback != null) callback.call(this, false);
                }
            },

            getActionNode: function() {
                /// <summary>Returns the latest node interacted with</summary>
                return this._actionNode;
            },

            setActiveTreeType: function(treeType) {
                /// <summary>
                /// All interactions with the tree are done so based on the current tree type (i.e. content, media).
                /// When sycning, or searching, the operations will be done on the current tree type so developers
                /// can explicitly specify on with this method before performing the operations.
                /// The active tree type is always updated any time a node interaction takes place.
                /// </summary>

                this._activeTreeType = treeType;
            },

            onNodeDeleting: function(EV) {
                /// <summary>Event handler for when a tree node is about to be deleted</summary>

                //first, close the branch
                this._tree.close_branch(this._actionNode.jsNode);
                //show the ajax loader with deleting text
                this._actionNode.jsNode.find("a").attr("class", "loading");
                this._actionNode.jsNode.find("a").css("background-image", "");
                this._actionNode.jsNode.find("a").html(this._uiKeys['deleting']);
            },

            onNodeDeleted: function(EV) {
                /// <summary>Event handler for when a tree node is deleted after ajax call</summary>

                //remove the ajax loader
                this._actionNode.jsNode.find("a").removeClass("loading");
                //ensure the branch is closed
                this._tree.close_branch(this._actionNode.jsNode);
                //make the node disapear
                this._actionNode.jsNode.hide("drop", { direction: "down" }, 400);

                this._updateRecycleBin();
            },

            onNodeRefresh: function(EV) {
                /// <summary>Handles the nodeRefresh event of the context menu and does the refreshing</summary>

                this._debug("onNodeRefresh");

                this._loadChildNodes(this._actionNode.jsNode, null);
            },

            onSelect: function(NODE, TREE_OBJ) {
                /// <summary>Fires the JS associated with the node</summary>

                this.setActiveTreeType($(NODE).attr("umb:type"));
                var js = $(NODE).children("a").attr("href").replace("javascript:", "");

                this._debug("onSelect: js: " + js);

                try {
                    var func = eval(js);
                    if (func != null) {
                        func.call();
                    }
                } catch (e) { }

                return true;
            },

            onOpen: function(NODE, TREE_OBJ) {
                /// <summary>
                /// Updates the image path references after data is returned from the server.
                /// Adds the overlay div to the markup if the specified style is found in the classes of this node.
                /// </summary>

                this._debug("onOpen: " + $(NODE).attr("id"));

                var nodes = $(NODE).find("ul > li");
                this._configureNodes(nodes);

                return true;
            },

            onBeforeOpen: function(NODE, TREE_OBJ) {
                /// <summary>Gets the node source from the meta data and assigns it to the tree prior to loading</summary>
                var nodeDef = this._getNodeDef($(NODE));
                this._debug("onBeforeOpen: " + nodeDef.nodeId);
                TREE_OBJ.settings.data.url = this._getUrl(nodeDef.sourceUrl);
            },

            onChange: function(NODE, TREE_OBJ) {
                /// <summary>
                /// Some code taken from the jsTree checkbox tree theme to allow for checkboxes
                /// </summary>
                if (this._treeType == "checkbox") {
                    var $this = $(NODE).is("li") ? $(NODE) : $(NODE).parent();
                    if ($this.children("a").hasClass("checked")) $this.children("a").removeClass("checked")
                    else $this.children("a").addClass("checked");
                }
                else if (this._treeType == "inheritedcheckbox") {
                    var $this = $(NODE).is("li") ? $(NODE) : $(NODE).parent();
                    if ($this.children("a.unchecked").size() == 0) {
                        TREE_OBJ.container.find("a").addClass("unchecked");
                    }
                    $this.children("a").removeClass("clicked");
                    if ($this.children("a").hasClass("checked")) {
                        $this.find("li").andSelf().children("a").removeClass("checked").removeClass("undetermined").addClass("unchecked");
                        var state = 0;
                    }
                    else {
                        $this.find("li").andSelf().children("a").removeClass("unchecked").removeClass("undetermined").addClass("checked");
                        var state = 1;
                    }
                    $this.parents("li").each(function() {
                        if (state == 1) {
                            if ($(this).find("a.unchecked, a.undetermined").size() - 1 > 0) {
                                $(this).parents("li").andSelf().children("a").removeClass("unchecked").removeClass("checked").addClass("undetermined");
                                return false;
                            }
                            else $(this).children("a").removeClass("unchecked").removeClass("undetermined").addClass("checked");
                        }
                        else {
                            if ($(this).find("a.checked, a.undetermined").size() - 1 > 0) {
                                $(this).parents("li").andSelf().children("a").removeClass("unchecked").removeClass("checked").addClass("undetermined");
                                return false;
                            }
                            else $(this).children("a").removeClass("checked").removeClass("undetermined").addClass("unchecked");
                        }
                    });
                }

                //bubble an event!
                $(this).trigger("nodeClicked", [NODE]);
            },

            onRightClick: function(NODE, TREE_OBJ, EV) {
                /// <summary>Builds the context menu based on the current node</summary>

                //update the action node's NodeDefinition and set the active tree type
                this._actionNode = this._getNodeDef($(NODE));
                this.setActiveTreeType($(NODE).attr("umb:type"));

                this._debug("onRightClick: menu = " + this._actionNode.menu);

                //remove the old menu from the DOM as jsTree will keep appending new ones.
                $("div").remove(".tree-default-context");

                if (this._actionNode.menu != "") {

                    //if there is a menu, then rebuilt the context menu
                    TREE_OBJ.settings.ui.context = this._getContextMenu(this._actionNode.menu);
                    TREE_OBJ.context_menu();

                    //now we need to bind events for hiding on a timer when lost focus
                    var timeout = null;
                    TREE_OBJ.context.mouseenter(function() {
                        clearTimeout(timeout);
                    });
                    TREE_OBJ.context.mouseleave(function() {
                        timeout = setTimeout(function() {
                            TREE_OBJ.hide_context();
                        }, 400);
                    });
                }
            },

            _debug: function(strMsg) {
                if (this._isDebug) {
                    Sys.Debug.trace("UmbracoTree: " + strMsg);
                }
            },

            _configureNodes: function(nodes, reconfigure) {
                /// <summary>
                /// Ensures the node is configured properly after it's loaded via ajax.
                /// This includes setting overlays and ensuring the correct icon paths are used.
                /// This also ensures that the correct markup is rendered for the tree (i.e. inserts html nodes for text, etc...)
                /// </summary>

                var _this = this;

                //don't process the nodes that have already been loaded, unless reconfigure is true
                if (!reconfigure) {
                    nodes = nodes.not("li[class*='loaded']");
                }

                this._debug("_configureNodes: " + nodes.length);

                var rxInput = new RegExp("\\boverlay-\\w+\\b", "gi");
                nodes.each(function() {
                    //if it is checkbox tree (not standard), don't worry about overlays and remove the default icon.
                    if (_this._treeType != "standard") {
                        $(this).children("a:first").css("background", "");
                        return;
                    }
                    //remove all overlays if reconfiguring
                    $(this).children("div").remove();
                    var m = $(this).attr("class").match(rxInput);
                    if (m != null) {
                        for (i = 0; i < m.length; i++) {
                            _this._debug("_configureNodes: adding overlay: " + m[i] + " for node: " + $(this).attr("id"));
                            $(this).children("a:first").before("<div class='overlay " + m[i] + "'></div>");
                        }
                    }
                    //create a div element inside the anchor and move the inner text into it
                    var txt = $(this).children("a").html();
                    $(this).children("a").html("<div>" + txt + "</div>");
                    //add the loaded class to each element so we know not to process it again
                    $(this).addClass("loaded");
                });
            },

            _getNodeDef: function(NODE) {
                /// <summary>Converts a jquery node with metadata to a NodeDefinition</summary>

                //get our meta data stored with our node
                var nodedata = $(NODE).children("a").metadata({ type: 'attr', name: 'umb:nodedata' });
                this._debug("_getNodeDef: " + $(NODE).attr("id") + ", " + nodedata.nodeType + ", " + nodedata.source);
                var def = new Umbraco.Controls.NodeDefinition();
                def.updateDefinition(this._tree, $(NODE), $(NODE).attr("id"), $(NODE).find("a > div").html(), nodedata.nodeType, nodedata.source, nodedata.menu, $(NODE).attr("umb:type"));
                return def;
            },

            _updateRecycleBin: function() {
                this._debug("_updateRecycleBin");

                var rNode = this.findNode(this._recycleBinId, true);
                if (rNode) {
                    this._actionNode = this._getNodeDef(rNode);
                    var _this = this;
                    this.reloadActionNode(true, true, function(success) {
                        if (success) {
                            _this.findNode(_this._recycleBinId, true).effect("highlight", {}, 1000);
                        }
                    });
                }
            },

            _loadChildNodes: function(liNode, callback) {
                /// <summary>jsTree won't allow you to open a node that doesn't explitly have childen, this will force it to try</summary>
                /// <param name="node">a jquery object for the current li node</param>

                this._debug("_loadChildNodes: " + liNode);

                liNode.removeClass("leaf");
                this._tree.close_branch(liNode, true);
                liNode.children("ul:eq(0)").html("");
                this._tree.open_branch(liNode, false, callback);
            },

            _syncTree: function(path, forceReload, numPaths, numAsync) {
                /// <summary>
                /// This is the internal method that will recursively search for the nodes to sync. If an invalid path is 
                /// passed to this method, it will raise an event which can be handled.
                /// </summary>
                /// <param name="path">The path of the node to find</param>
                /// <param name="forceReload">If true, will ensure that the node to be synced is synced with data from the server</param>
                /// <param name="numPaths">the number of id's deep to search starting from the end of the path. Used in recursion.</param>
                /// <param name="numAsync">the number of async calls made so far to sync. Used in recursion and used to determine if the found node has been loaded by ajax.</param>

                this._debug("_syncTree");

                var paths = path.split(",");
                var found = null;
                var foundIndex = null;
                if (numPaths == null) numPaths = (paths.length - 0);
                for (var i = 0; i < numPaths; i++) {
                    foundIndex = paths.length - (1 + i);
                    found = this.findNode(paths[foundIndex]);
                    this._debug("_syncTree: finding... " + paths[foundIndex] + " found? " + found);
                    if (found) break;
                }

                //if no node has been found at all in the entire path, then bubble an error event
                if (!found) {
                    this._debug("no node found in path: " + path + " : " + numPaths);
                    $(this).trigger("syncNotFound", [path]);
                    return;
                }

                //if the found node was not the end of the path, we need to load them in recursively.
                if (found.attr("id") != paths[paths.length - 1]) {
                    var _this = this;
                    this._loadChildNodes(found, function(NODE, TREE_OBJ) {
                        //check if the next node to be found is in the children, if it is not, there's a problem bubble an event!
                        var pathsToSearch = paths.length - (Number(foundIndex) + 1);
                        if (_this.findNode(paths[foundIndex + 1])) {
                            _this._syncTree(path, forceReload, pathsToSearch, (numAsync == null ? numAsync == 1 : ++numAsync));
                        }
                        else {
                            _this._debug("node not found in children: " + path + " : " + numPaths);
                            $(this).trigger("syncNotFound", [path]);
                        }
                    });
                }
                else {
                    //only force the reload of this nodes data if forceReload is specified and the node has not already come from the server
                    var doReload = (forceReload && (numAsync == null || numAsync < 1));
                    this._debug("_syncTree: found! numAsync: " + numAsync + ", forceReload: " + forceReload);
                    if (doReload) {
                        this._actionNode = this._getNodeDef(found);
                        this.reloadActionNode(true, true, null);
                    }
                    else {
                        //we have found our node, select it but supress the selecting event
                        if (found.attr("id") != "-1") this.selectNode(found, true);
                        this._configureNodes(found, doReload);
                    }
                    //bubble event
                    $(this).trigger("syncFound", [found]);
                }
            },

            _getContextMenu: function(strMenu) {
                /// <summary>Builds a new context menu object (array) based on the string representation passed in</summary>

                this._debug("_getContextMenu: " + strMenu);

                var newMenu = new Array();
                for (var i = 0; i < strMenu.length; i++) {
                    var letter = strMenu.charAt(i);
                    //get a js menu item by letter
                    var menuItem = this._getMenuItemByLetter(letter);
                    if (menuItem != null) newMenu.push(menuItem);
                }
                return newMenu;
            },

            _getMenuItemByLetter: function(letter) {
                /// <summary>Finds the menu item in our full menu by the letter and returns object</summary>

                //insert selector if it's a comma
                if (letter == ",") return "separator";
                for (var m in this._fullMenu) {
                    if (this._fullMenu[m].id == letter) {
                        return this._fullMenu[m];
                    }
                }
                return null;
            },

            _init: function(jFullMenu, jInitNode, treeContainer, appActions, uiKeys, app, showContext, isDialog, treeType, serviceUrl, dataUrl) {
                /// <summary>initialization method, must be called on page ready.</summary>
                /// <param name="jFullMenu">JSON markup for the full context menu in accordance with the jsTree context menu object standard</param>
                /// <param name="jInitNode">JSON markup for the initial node to show</param>
                /// <param name="treeContainer">the jQuery element to be tree enabled</param>
                /// <param name="appActions">A reference to a MenuActions object</param>
                /// <param name="uiKeys">A reference to a uiKeys object</param>
                /// <param name="showContext">boolean indicating whether or not to show a context menu</param>
                /// <param name="isDialog">boolean indicating whether or not the tree is in dialog mode</param>
                /// <param name="treeType">determines the type of tree: false/null = normal, 'checkbox' = checkboxes enabled, 'inheritedcheckbox' = parent nodes have checks inherited from children</param>
                /// <param name="serviceUrl">Url path for the tree client service</param>
                /// <param name="dataUrl">Url path for the tree data service</param>

                this._debug("init: creating new tree with class/id: " + treeContainer.attr("class") + " / " + treeContainer.attr("id"));

                this._fullMenu = jFullMenu;
                this._initNode = jInitNode;
                this._menuActions = appActions;
                this._uiKeys = uiKeys;
                this._app = app;
                this._showContext = showContext;
                this._isDialog = isDialog;
                this._treeType = treeType;
                this._serviceUrl = serviceUrl;
                this._dataUrl = dataUrl;

                //wire up event handlers
                if (this._menuActions != null) {
                    var _this = this;
                    //wrapped functions maintain scope
                    this._menuActions.addEventHandler("nodeDeleting", function(E) { _this.onNodeDeleting(E) });
                    this._menuActions.addEventHandler("nodeDeleted", function(E) { _this.onNodeDeleted(E) });
                    this._menuActions.addEventHandler("nodeRefresh", function(E) { _this.onNodeRefresh(E) });
                }

                //initializes the jsTree
                this._container = treeContainer;
                this._tree = $.tree_create();
                this._tree.init(this._container, this._getInitOptions());

                //load child nodes of the init node
                this._loadChildNodes(this._container.find("li:first"), null);
            },

            _getUrl: function(nodeSource) {
                /// <summary>Returns the json service url</summary>

                if (nodeSource == null || nodeSource == "") {
                    return this._dataUrl;
                }
                var params = nodeSource.split("?")[1];
                return this._dataUrl + "?" + params + "&rnd2=" + Umbraco.Utils.generateRandom();
            },

            _getInitOptions: function() {
                /// <summary>return the initialization objects for the tree</summary>

                var _this = this;

                var options = {
                    data: {
                        type: "json",
                        async: true,
                        url: "",
                        json: this._initNode,
                        async_data: function(NODE) { return null; } //ensures that the node id isn't appended to the async url
                    },
                    ui: {
                        dots: false,
                        rtl: false,
                        animation: false,
                        hover_mode: true,
                        theme_path: "/umbraco_client/Tree/Themes/",
                        theme_name: "umbraco",
                        context: null //no context menu by default						
                    },
                    rules: {
                        metadata: "umb:nodedata",
                        creatable: "none"
                    },
                    callback: {
                        //wrapped functions maintain scope in callback
                        onrgtclk: function(N, T, E) { _this.onRightClick(N, T, E) },
                        beforeopen: function(N, T) { _this.onBeforeOpen(N, T) },
                        onopen: function(N, T) { _this.onOpen(N, T) },
                        onselect: function(N, T) { _this.onSelect(N, T) },
                        onchange: function(N, T) { _this.onChange(N, T) }
                    }
                };

                return options;
            }

        };
    }
})(jQuery);