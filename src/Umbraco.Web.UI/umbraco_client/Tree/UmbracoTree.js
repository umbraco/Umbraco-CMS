/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference path="/umbraco_client/Application/UmbracoUtils.js" />
/// <reference path="/umbraco_client/ui/jquery.js" />
/// <reference path="/umbraco_client/ui/jqueryui.js" />
/// <reference path="tree_component.js" />
/// <reference path="/umbraco_client/Application/UmbracoApplicationActions.js" />
/// <reference path="NodeDefinition.js" />
/// <reference name="MicrosoftAjax.js"/>

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {

    $.fn.UmbracoTree = function(opts) {
        /// <summary>jQuery plugin to create an Umbraco tree. See option remarks below.</summary>
        return this.each(function() {
            new Umbraco.Controls.UmbracoTree().init($(this), opts);
        });
    };
    $.fn.UmbracoTreeAPI = function() {
        /// <summary>exposes the Umbraco Tree api for the selected object</summary>
        if ($(this).length != 1) {
            throw "UmbracoTreeAPI selector requires that there be exactly one control selected, this selector returns " + $(this).length;
        };
        // check if there's an api stored for the id of the object specified, if there's not
        // check if the first child is a div and if that has the api specified
        var api = Umbraco.Controls.UmbracoTree.inst[$(this).attr("id")] || null;
        if (api == null)
            return Umbraco.Controls.UmbracoTree.inst[$(this).children("div").attr("id")] || null;
        return api;
    };

    Umbraco.Controls.TreeDefaultOptions = function() {
        return {
            doNotInit: false, //this is used for the main umbraco tree so that the tree doesn't fully initialize until rebuildTree is explicitly called
            jsonFullMenu: {}, //The tree menu, by default is empty
            appActions: null, //A reference to a MenuActions object
            deletingText: "Deleting...", //the txt to display when a node is deleting
            treeMode: "standard", //determines the type of tree: false/null = normal, 'checkbox' = checkboxes enabled, 'inheritedcheckbox' = parent nodes have checks inherited from children
            recycleBinId: -20, //the id of the recycle bin for the current tree
            serviceUrl: "", //Url path for the tree client service
            dataUrl: "", //Url path for the tree data service

            //These are all properties of the ITreeService and are
            //used to pass the properties in to the InitAppTreeData service
            app: "", //the application name to render
            treeType: "", //the active tree application
            showContext: true, //boolean indicating whether or not to show a context menu
            isDialog: false,
            dialogMode: "none", //boolean indicating whether or not the tree is in dialog mode
            functionToCall: "",
            nodeKey: ""
        };
    };

    Umbraco.Controls.UmbracoTree = function() {
        /// <summary>
        /// The object that manages the Umbraco tree.
        /// Has these events: syncNotFound, syncFound, rebuiltTree, newchildNodeFound, nodeMoved, nodeCopied, ajaxError, nodeClicked
        /// </summary>
        return {
            _opts: {},
            _cntr: ++Umbraco.Controls.UmbracoTree.cntr, //increments the number of tree instances.
            _containerId: null,
            _context: null, //the jquery context used to get the container
            _actionNode: new Umbraco.Controls.NodeDefinition(), //the most recent node right clicked for context menu
            _activeTreeType: "content", //tracks which is the active tree type, this is used in searching and syncing.
            _tree: null, //reference to the jsTree object
            _isEditMode: false, //not really used YET
            _isDebug: false, //set to true to enable alert debugging
            _loadedApps: [], //stores the application names that have been loaded to track which JavaScript code has been inserted into the DOM
            _treeClass: "umbTree", //used for other libraries to detect which elements are an umbraco tree
            _currenAJAXRequest: false, //used to determine if there is currently an ajax request being executed.
            _isSyncing: false,

            addEventHandler: function(fnName, fn) {
                /// <summary>Adds an event listener to the event name event</summary>
                this._getContainer().bind(fnName, fn);
            },

            removeEventHandler: function(fnName, fn) {
                /// <summary>Removes an event listener to the event name event</summary>
                this._getContainer().unbind(fnName, fn);
            },

            _raiseEvent: function(evName, args) {
                /// <summary>Raises an event and attaches it to the container</summary>
                this._getContainer().trigger(evName, args);
            },

            init: function(jItem, opts) {
                /// <summary>Initializes the tree with the options and stores the tree API in the jQuery data object for the current element</summary>
                this._debug("init: creating new tree with class/id: " + jItem.attr("class") + " / " + jItem.attr("id"));

                this._opts = $.extend(Umbraco.Controls.TreeDefaultOptions(), opts);

                this._context = jItem.get(0).ownerDocument;

                //wire up event handlers
                if (this._opts.appActions != null) {
                    var _this = this;
                    //wrapped functions maintain scope
                    this._opts.appActions.addEventHandler("nodeDeleting", function (E) { _this.onNodeDeleting(E); });
                    this._opts.appActions.addEventHandler("nodeDeleted", function (E) { _this.onNodeDeleted(E); });
                    this._opts.appActions.addEventHandler("nodeRefresh", function (E) { _this.onNodeRefresh(E); });
                    this._opts.appActions.addEventHandler("publicError", function (E, err) { _this.onPublicError(E, err); });
                }

                this._containerId = jItem.attr("id");

                if (!this._opts.doNotInit) {
                    //initializes the jsTree                    
                    this._tree = $.tree.create();
                    this._tree.init(this._getContainer(), this._getInitOptions());
                }

                jItem.addClass(this._treeClass);

                //store a reference to this api by the id and the counter
                Umbraco.Controls.UmbracoTree.inst[this._cntr] = this;
                if (!this._getContainer().attr("id")) this._getContainer().attr("id", "UmbTree_" + this._cntr);
                Umbraco.Controls.UmbracoTree.inst[this._getContainer().attr("id")] = Umbraco.Controls.UmbracoTree.inst[this._cntr];

            },

            setRecycleBinNodeId: function(id) {
                this._opts.recycleBinId = id;
            },

            //TODO: add public method to clear a specific tree cache
            clearTreeCache: function() {
                // <summary>This will remove all stored trees in client side cache so that the next time a tree needs loading it will be refreshed</summary>
                this._debug("clearTreeCache...");

                this._loadedApps = [];
            },

            toggleEditMode: function(enable) {
                this._debug("Edit mode. Currently: " + this._tree.settings.rules.draggable);
                this._isEditMode = enable;

                this.saveTreeState(this._opts.app);
                //need to trick the system so it thinks it's a different app, then rebuild with new rules
                var app = this._opts.app;
                this._opts.app = "temp";
                this.rebuildTree(app);

                this._debug("Edit mode. New Mode: " + this._tree.settings.rules.draggable);
                if (this._opts.appActions)
                    this._opts.appActions.showSpeachBubble("info", "Tree Edit Mode", "The tree is now operating in edit mode");
            },

            refreshTree: function(treeType) {
                /// <summary>This wraps the standard jsTree functionality unless a treeType is specified. If one is, then it will just reload that nodes children</summary>
                this._debug("refreshTree: " + treeType);
                if (!treeType) {
                    this.rebuildTree();
                }
                else {
                    var allRoots = this._getContainer().find("li[rel='rootNode']");
                    var _this = this;
                    var root = allRoots.filter(function() {
                        return ($(this).attr("umb:type") == _this._activeTreeType); //filter based on custom namespace requires custom function
                    });
                    if (root.length == 1) {
                        this._debug("refreshTree: reloading tree type: " + treeType);
                        this._loadChildNodes(root);
                    }
                    else {
                        //couldn't find it, so refresh the whole tree
                        this.rebuildTree();
                    }
                }

            },
            rebuildTree: function(app, callback) {
                /// <summary>This will rebuild the tree structure for the application specified</summary>

                this._debug("rebuildTree");

                //if app is null, then we will rebuild the current app which also means clearing the cache.
                if (!app) {                                        
                    this.clearTreeCache();
                    app = this._opts.app; // zb-00011 #29447 : bugfix assignment
                }
                else if (this._tree && (this._opts.app.toLowerCase() == app.toLowerCase())) {
                    this._debug("not rebuilding");
                    
                    //don't rebuild if the tree object exists, the app that's being requested to be loaded is 
                    //flagged as already loaded, and the tree actually has nodes in it
                    
                    return;
                }
                else {
                    this._opts.app = app;
                }
                //kill the tree
                if (this._tree) {
                    this._tree.destroy();
                }

                var _this = this;

                //check if we should rebuild from a saved tree
                var saveData = this._loadedApps["tree_" + app];

                this.setActiveTreeType(app);

                if (saveData != null) {
                    this._debug("rebuildTree: rebuilding from cache: app = " + app);

                    //create the tree from the saved data.
                    //this._initNode = saveData.d;
                    this._tree = $.tree.create();
                    this._tree.init(this._getContainer(), this._getInitOptions(saveData.d));

                    //ensure the static data is gone
                    this._tree.settings.data.opts.static = null;

                    this._configureNodes(this._getContainer().find("li"), true);
                    //select the last node
                    var lastSelected = saveData.selected != null ? $(saveData.selected[0]).attr("id") : null;
                    if (lastSelected != null) {
                        //create an event handler for the tree sync
                        var _this = this;
                        var foundHandler = function(EV, node) {
                            //remove the event handler from firing again
                            _this.removeEventHandler("syncFound", foundHandler);
                            _this._debug("rebuildTree: node synced, selecting node...");
                            //ensure the node is selected, ensure the event is fired and reselect is true since jsTree thinks this node is already selected by id
                            _this.selectNode(node, false, true);
                        };
                        this._debug("rebuildTree: syncing to last selected: " + lastSelected);
                        //add the event handler for the tree sync and sync the tree
                        this.addEventHandler("syncFound", foundHandler);
                        this.setActiveTreeType($(saveData.selected[0]).attr("umb:type"));                      
                        this.syncTree(lastSelected);
                    }

                    if (typeof callback == "function") callback.apply(this, [lastSelected]);
                }
                else {
                    this._debug("rebuildTree: rebuilding from scratch: app = " + app);

                    this._currentAJAXRequest = true;

                    _this._tree = $.tree.create();
                    _this._tree.init(_this._getContainer(), _this._getInitOptions());

                    if (typeof callback == "function") callback.apply(this, []);
                }

            },

            saveTreeState: function(appAlias) {
                /// <summary>
                /// Saves the state of the current application trees so we can restore it next time the user visits the app
                /// </summary>

                this._debug("saveTreeState: " + appAlias + " : ajax request? " + this._currentAJAXRequest);

                //clear the saved data for the current app before saving
                this._loadedApps["tree_" + appAlias] = null;

                //if an ajax request is currently in progress, abort saving the tree state and set the 
                //data object for the application to null.
                if (!this._currentAJAXRequest) {
                    //only save the data if there are nodes
                    var nodeCount = this._getContainer().find("li[rel='dataNode']").length;
                    if (nodeCount > 0) {
                        this._debug("saveTreeState: node count = " + nodeCount);
                        var treeData = this._tree.get();
                        //need to update the 'state' of the data. jsTree get doesn't return the state of nodes properly!
                        this._updateJSONNodeState(treeData);
                        this._debug("saveTreeState: treeData = " + treeData);

                        this._loadedApps["tree_" + appAlias] = { selected: this._tree.selected, d: treeData };
                    }
                }
            },

            _updateJSONNodeState: function(obj) {
                /// <summary>
                /// A recursive function to store the state of the node for the JSON object when using saveTreeState.
                /// This is required since jsTree doesn't output the state of the tree nodes with the request to getJSON method.
                /// This is also required to save the correct title for each node since we store our title in a div tag, not just the a tag
                /// </summary>              

                var node = $("li[id='" + obj.attributes.id + "']").filter(function() {
                    return ($(this).attr("umb:type") == obj.attributes["umb:type"]); //filter based on custom namespace requires custom function
                });

                //saves the correct title
                obj.data.title = $.trim(node.children("a").children("div").text());
                obj.state = obj.data.state;
                //ensures that the style property of the data contains only single quotes since jsTree puts double around the attr
                for (var i in obj.data.attributes) {
                    if (!obj.data.attributes.hasOwnProperty(i)) continue;
                    if (i == "style" || i == "class") {
                        obj.data.attributes[i] = obj.data.attributes[i].replace(/\"/g, "'");
                    }
                }

                //recurse through children
                if (obj.children != null) {
                    for (var x in obj.children) {
                        this._updateJSONNodeState(obj.children[x]);
                    }
                }
            },

            syncTree: function(path, forceReload, supressChildReload, newId) {
                /// <summary>
                /// Syncronizes the tree with the path supplied and makes that node visible/selected.
                /// </summary>
                /// <param name="path">The path of the node</param>
                /// <param name="forceReload">If true, will ensure that the node to be synced is synced with data from the server</param>
                /// <param name="newId">This parameter is only used when we don't have a real unique ID for a node, for example for a file. If a filename changes we don't know what the new one is since we are syncing the tree to the old original path. Once we retrieve the results the sync the tree we need to find the result by it's new id and update the node.</param>

                this._debug("syncTree: " + path + ", " + forceReload);

                //set the flag so that multiple synces aren't attempted
                this._isSyncing = true;

                this._syncTree.call(this, path, forceReload, null, null, supressChildReload, newId);

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
                            _this._raiseEvent("newChildNodeFound", [found]);
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
                    _this._raiseEvent("nodeMoved", [node]);
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
                    _this._raiseEvent("nodeCopied", [node]);
                };
                //add the event handler for the tree sync and sync the to the parent path
                this.addEventHandler("syncFound", foundHandler);
                this.syncTree(parentPath);
            },

            findNode: function(nodeId, findGlobal) {
                /// <summary>Returns either the found branch or false if not found in the tree</summary>
                /// <param name="findGlobal">Optional. If true, disregards the tree type and searches the entire tree for the id</param>
                var _this = this;
                var branch = this._getContainer().find("li[id='" + nodeId + "']");
                if (!findGlobal) branch = branch.filter(function() {
                    return ($(this).attr("umb:type") == _this._activeTreeType); //filter based on custom namespace requires custom function
                });
                var found = branch.length > 0 ? branch : false;
                this._debug("findNode: " + nodeId + " in '" + this._activeTreeType + "' tree. Found? " + found);
                return found;
            },

            selectNode: function(node, supressEvent, reselect) {
                /// <summary>
                /// Makes the selected node the active node, but only if it is not already selected or if reselect is true.            
                /// </summary>
                /// <param name="supressEvent">If set to true, will select the node but will supress the onSelected event</param>
                /// <param name="reselect">If set to true, will call the select_branch method even if the node is already selected</param>

                //this._debug("selectNode, edit mode? " + this._isEditMode);

                var selectedId = this._tree.selected != null ? $(this._tree.selected[0]).attr("id") : null;

                this._debug("selectNode (" + node.attr("id") + "). supressEvent? " + supressEvent + ", reselect? " + reselect);

                if (reselect || (selectedId == null || selectedId != node.attr("id"))) {
                    //if we don't wan the event to fire, we'll set the callback to a null method and set it back after we call the select_branch method
                    if (supressEvent || this._isEditMode) {
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
                        var nodeDef = this.getNodeDef(nodeParent);
                        this._debug("reloadActionNode: loading ajax for node: " + nodeDef.nodeId);
                        var _this = this;
                        //replace the node to refresh with loading and return the new loading element
                        var toReplace = $("<li class='last'><a class='loading' href='#'><ins></ins><div>" + (this._tree.settings.lang.loading || "Loading ...") + "</div></a></li>").replaceAll(this._actionNode.jsNode);
                        $.get(this._getUrl(nodeDef.sourceUrl), null,
                            function(msg) {
                                if (!msg || msg.length == 0) {
                                    _this._debug("reloadActionNode: error loading ajax data, performing jsTree refresh");
                                    _this.rebuildTree(); /*try jsTree refresh as last resort */
                                    if (callback != null) callback.call(_this, false);
                                    return;
                                }
                                //filter the results to find the object corresponding to the one we want refreshed
                                var oFound = null;
                                for (var o in msg) {
                                    if (msg[o].attributes != null && msg[o].attributes.id == _this._actionNode.nodeId) {
                                        oFound = $.tree.datastores.json().parse(msg[o], _this._tree);
                                        //ensure the tree type is the same too
                                        if ($(oFound).attr("umb:type") == _this._actionNode.treeType) { break; }
                                        else { oFound = null; }
                                    }
                                }
                                if (oFound != null) {
                                    _this._debug("reloadActionNode: node is refreshed! : " + supressSelect);
                                    var reloaded = $(oFound).replaceAll(toReplace);
                                    _this._configureNodes(reloaded, true);
                                    if (!supressSelect) _this.selectNode(reloaded, true, true);
                                    if (!supressChildReload) {
                                        _this._loadChildNodes(reloaded, function() {
                                            if (callback != null) callback.call(_this, true);
                                        });
                                    }
                                    else { if (callback != null) callback.call(_this, true); }
                                }
                                else {
                                    _this._debug("reloadActionNode: error finding child node in ajax data, performing jsTree refresh");
                                    _this.rebuildTree(); /*try jsTree refresh as last resort */
                                    if (callback != null) callback.call(_this, false);
                                }
                            }, "json");
                        return;
                    }

                    this._debug("reloadActionNode: error finding parent node, performing jsTree refresh");
                    this.rebuildTree(); /*try jsTree refresh as last resort */
                    if (callback != null) callback.call(this, false);
                }
            },

            getActionNode: function() {
                /// <summary>Returns the latest node interacted with</summary>
                this._debug("getActionNode: " + this._actionNode.nodeId);
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

                this._debug("onNodeDeleting")

                //first, close the branch
                this._tree.close_branch(this._actionNode.jsNode);
                //show the deleting text
                this._actionNode.jsNode.find("a div")
                    .html(this._opts.deletingText)
                    .effect("highlight", {}, 1000);
            },

            onNodeDeleted: function (EV) {
                /// <summary>Event handler for when a tree node is deleted after ajax call</summary>

                this._debug("onNodeDeleted");

                var tree = this._tree;
                var nodeToDel = this._actionNode.jsNode;
                var parentNode = this._tree.parent(nodeToDel);

                //ensure the branch is closed
                this._tree.close_branch(nodeToDel);
                //make the node disapear
                nodeToDel.hide("drop", { direction: "down" }, 400, function () {
                    //remove the node from the DOM, do this after 1 second as IE doesn't like it when you try this right away.
                    setTimeout(function () {
                        nodeToDel.remove();
                        
                        if (parentNode != undefined && parentNode != -1) {
                            tree.open_branch(parentNode);
                        }
                        
                    }, 250);
                });
                
                this._updateRecycleBin();
                
            },

            onNodeRefresh: function(EV) {
                /// <summary>Handles the nodeRefresh event of the context menu and does the refreshing</summary>

                this._debug("onNodeRefresh");

                this._loadChildNodes(this._actionNode.jsNode, null);
            },

            onSelect: function(NODE, TREE_OBJ) {
                /// <summary>Fires the JS associated with the node, if the tree is in edit mode, allows for rename instead</summary>
                //this._debug("onSelect, edit mode? " + this._isEditMode);
                this._debug("onSelect");
                if (this._isEditMode) {
                    this._tree.rename(NODE);
                    return false;
                }
                else {
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
                }


            },

            onBeforeOpen: function(NODE, TREE_OBJ) {
                /// <summary>Before opening child nodes, ensure that the data method and url are set properly</summary>
                this._currentAJAXRequest = true;
                TREE_OBJ.settings.data.opts.url = this._opts.dataUrl;
                TREE_OBJ.settings.data.opts.method = "GET";
            },

            onJSONData: function(DATA, TREE_OBJ) {
                this._debug("onJSONData");

                this._ensureContext();

                this._currentAJAXRequest = false;

                if (typeof DATA.d != "undefined") {

                    var msg = DATA.d;
                    //recreates the tree
                    if ($.inArray(msg.app, this._loadedApps) == -1) {
                        this._debug("loading js for app: " + msg.app);
                        this._loadedApps.push(msg.app);
                        //inject the scripts
                        this._getContainer().after("<script>" + msg.js + "</script>");
                    }
                    return eval(msg.json);
                }

                return DATA;
            },

            onBeforeRequest: function(NODE, TREE_OBJ) {
                this._ensureContext();

                if (TREE_OBJ.settings.data.opts.method == "POST") {
                    var parameters = "{'app':'" + this._opts.app + "','showContextMenu':'" + this._opts.showContext + "', 'isDialog':'" + this._opts.isDialog + "', 'dialogMode':'" + this._opts.dialogMode + "', 'treeType':'" + this._opts.treeType + "', 'functionToCall':'" + this._opts.functionToCall + "', 'nodeKey':'" + this._opts.nodeKey + "'}"
                    return parameters;
                }
                else {
                    var nodeDef = this.getNodeDef($(NODE));
                    return this._getUrlParams(nodeDef.sourceUrl);
                }
            },

            onChange: function(NODE, TREE_OBJ) {
                //bubble an event!
                this._raiseEvent("nodeClicked", [NODE]);
            },

            onBeforeContext: function(NODE, TREE_OBJ, EV) {

                //update the action node's NodeDefinition and set the active tree type
                this._actionNode = this.getNodeDef($(NODE));
                this.setActiveTreeType($(NODE).attr("umb:type"));

                this._debug("onBeforeContext: " + this._actionNode.menu);

                return this._actionNode.menu;
            },

            onLoad: function(TREE_OBJ) {
                /// <summary>When the application first loads, load the child nodes</summary>

                this._debug("onLoad");

                //ensure the static data is gone
                this._tree.settings.data.opts.static = null;
                var _this = this;
                _this._loadChildNodes($(_this._getContainer()).find("li"), null);
            },

            onParse: function(STR, TREE_OBJ) {
                this._debug("onParse");

                this._ensureContext();

                var obj = $(STR);
                this._configureNodes(obj);
                //this will return the full html of the configured node
                return $('<div>').append($(obj).clone()).remove().html();
            },

            onDestroy: function(TREE_OBJ) {
                /// <summary>
                /// When the tree is destroyed we need to ensure that all of the events both
                /// live and bind are gone. For some reason the jstree unbinding doesn't seem to do it's job
                /// so instead we need to clear all of the events ourselves
                /// </summary>
                this._debug("onDestroy: " + TREE_OBJ.container.attr("id"));

                TREE_OBJ.container
                    .unbind("contextmenu")
                    .unbind("click")
                    .unbind("dblclick")
                    .unbind("mouseover")
                    .unbind("mousedown")
                    .unbind("mouseup");

                $("a", TREE_OBJ.container.get(0))
                    .die("contextmenu")
                    .die("click")
                    .die("dblclick")
                    .die("mouseover")
                    .die("mousedown");

                //also need to kill the custom selector we've fixed in jstree source
                $("#" + TREE_OBJ.container.attr("id") + " li").die("click");

                $("li", TREE_OBJ.container.get(0))
                    .die("click");
            },

            onError: function(ERR, TREE_OBJ) {
                this._debug("ERROR!!!!! " + ERR);
            },
            
            onPublicError: function(ev, errorObj) {
                /// <summary>Event handler for when a tree node fails an ajax call</summary>

                this._debug("onPublicError");

                var errorNode = this._actionNode.jsNode;

                // reload parent
                this.reloadActionNode(false, true, null);

                if (this._isDebug) {
                    alert('There was an error processing the request\n' +
                          '=========================================\n\n' +
                          'Error Message:\n ' +
                          errorObj.get_message() + '\n\n' +
                          'Technical information:\n ' +
                          '=========================================\n\n' +
                          'Status Code: ' + errorObj.get_statusCode() + '\n\n' +
                          'Exception Type: ' + errorObj.get_exceptionType() + '\n\n' +
                          'Timed Out: ' + errorObj.get_timedOut() + '\n\n' +
                          'Full Stacktrace:\n' + errorObj.get_stackTrace());
                } else {
                    this._opts.appActions.showSpeachBubble("error", "Error handling action", errorObj.get_message());
                }

            },

            _debug: function(strMsg) {
                if (this._isDebug && Sys && Sys.Debug) {
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
                    if (_this._opts.treeMode != "standard") {
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
                    //create a div for the text
                    var a = $(this).children("a");
                    var ins = a.children("ins");
                    ins.remove(); //need to remove before you do a .text() otherwise whitespace is included
                    var txt = $("<div>" + a.text() + "</div>");
                    //check if it's not a sprite, if not then move the ins node just after the anchor, otherwise remove                    
                    if (a.hasClass("noSpr")) {
                        a.attr("style", ins.attr("style"));
                    }
                    else {

                    }
                    a.html(txt);
                    //add the loaded class to each element so we know not to process it again
                    $(this).addClass("loaded");
                });
            },

            getNodeDef: function(NODE) {
                /// <summary>Converts a jquery node with metadata to a NodeDefinition</summary>

                //get our meta data stored with our node
                var nodedata = $(NODE).children("a").metadata({ type: 'attr', name: 'umb:nodedata' });
                this._debug("getNodeDef: " + $(NODE).attr("id") + ", " + nodedata.nodeType + ", " + nodedata.source);
                var def = new Umbraco.Controls.NodeDefinition();
                def.updateDefinition(this._tree, $(NODE), $(NODE).attr("id"), $(NODE).find("a > div").html(), nodedata.nodeType, nodedata.source, nodedata.menu, $(NODE).attr("umb:type"));
                return def;
            },

            _updateRecycleBin: function() {
                /// <summary>Generally used for when a node is deleted. This will set the actionNode to the recycle bin node and force a refresh of it's children</summary>
                this._debug("_updateRecycleBin BinId: " + this._opts.recycleBinId);

                var rNode = this.findNode(this._opts.recycleBinId, true);
                if (rNode) {
                    this._actionNode = this.getNodeDef(rNode);
                    var _this = this;
                    this.reloadActionNode(true, true, function(success) {
                        if (success) {
                            _this.findNode(_this._opts.recycleBinId, true).effect("highlight", {}, 1000);
                        }
                    });
                }
            },
            _ensureContext: function() {
                /// <summary>
                /// ensure that the tree object always has the correct context.
                /// this is a fix for the TinyMCE dialog window, as it tends to lose object context for some wacky reason
                /// when ajax calls are made. Works fine in all other instances.
                /// </summary>
                this._tree.container = this._getContainer();
            },
            _loadChildNodes: function(liNode, callback) {
                /// <summary>jsTree won't allow you to open a node that doesn't explitly have childen, this will force it to try</summary>
                /// <param name="node">a jquery object for the current li node</param>

                this._debug("_loadChildNodes: " + liNode.attr("id"));

                liNode.removeClass("leaf");
                
                var _this = this;
                
                //close branch will actually cause a select to happen so we'll intercept the select callback and then reset it once complete
                //if we don't wan the event to fire, we'll set the callback to a null method and set it back after we call the select_branch method               
                this._tree.settings.callback.onselect = function() { };
                this._tree.close_branch(liNode, true);                
                this._tree.settings.callback.onselect = function(N, T) { _this.onSelect(N, T) };

                liNode.children("ul:eq(0)").remove();
                this._tree.open_branch(liNode, false, callback);
            },
            
            _syncTree: function(path, forceReload, numPaths, numAsync, supressChildReload, newId) {
                /// <summary>
                /// This is the internal method that will recursively search for the nodes to sync. If an invalid path is 
                /// passed to this method, it will raise an event which can be handled.
                /// </summary>
                /// <param name="path">The path of the node to find</param>
                /// <param name="forceReload">If true, will ensure that the node to be synced is synced with data from the server</param>
                /// <param name="numPaths">the number of id's deep to search starting from the end of the path. Used in recursion.</param>
                /// <param name="numAsync">the number of async calls made so far to sync. Used in recursion and used to determine if the found node has been loaded by ajax.</param>
                /// <param name="newId">This parameter is only used when we don't have a real unique ID for a node, for example for a file. If a filename changes we don't know what the new one is since we are syncing the tree to the old original path. Once we retrieve the results the sync the tree we need to find the result by it's new id and update the node.</param>

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
                    this._isSyncing = false; //reset flag
                    this._raiseEvent("syncNotFound", [path]);
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
                            this._isSyncing = false; //reset flag
                            _this._raiseEvent("syncNotFound", [path]);
                        }
                    });
                }
                else {
                    //only force the reload of this nodes data if forceReload is specified and the node has not already come from the server
                    var doReload = (forceReload && (numAsync == null || numAsync < 1));
                    this._debug("_syncTree: found! numAsync: " + numAsync + ", forceReload: " + forceReload + ", doReload: " + doReload);                                        
                    if (doReload) {
                        this._actionNode = this.getNodeDef(found);
                        //we need to change the id if the newId parameter is set
                        if (newId) {
                            this._actionNode.nodeId = newId;
                        }                        
                        if (supressChildReload === undefined) {
                            this.reloadActionNode(false, true, null);
                        } else {
                            this.reloadActionNode(false, supressChildReload, null); 
                        }
                    }
                    else {
                        //we have found our node, select it but supress the selecting event
                        if (found.attr("id") != "-1") this.selectNode(found, true);
                        this._configureNodes(found, doReload);
                    }
                    this._isSyncing = false; //reset flag
                    //bubble event
                    this._raiseEvent("syncFound", [found]);
                }
            },

            
            _getUrlParams: function(nodeSource) {
                /// <summary>This converts Url query string params to json</summary>
                var p = {};
                if (nodeSource) {
                    var urlSplit = nodeSource.split("?");
                    if (urlSplit.length > 1) {
                        var sp = urlSplit[1].split("&");
                        for (var i = 0; i < sp.length; i++) {
                            var e = sp[i].split("=");
                            p[e[0]] = e[1];
                        }
                        p["rnd2"] = Umbraco.Utils.generateRandom();                    
                    }
                }                
                return p;
            },

            _getUrl: function(nodeSource) {
                /// <summary>Returns the json service url</summary>

                if (nodeSource == null || nodeSource == "") {
                    return this._opts.dataUrl;
                }
                var params = nodeSource.split("?")[1];
                return this._opts.dataUrl + "?" + params + "&rnd2=" + Umbraco.Utils.generateRandom();
            },
            _getContainer: function() {
                return $("#" + this._containerId, this._context);
            },
            _getInitOptions: function(initData) {
                /// <summary>return the initialization objects for the tree</summary>

                this._debug("_getInitOptions");

                var _this = this;

                var options = {
                    data: {
                        type: "json",
                        async: true,
                        opts: {
                            static: initData == null ? null : initData,
                            method: "POST",
                            url: _this._opts.serviceUrl,
                            outer_attrib: ["id", "umb:type", "class", "rel"],
                            inner_attrib: ["umb:nodedata", "href", "class", "style"]
                        }
                    },
                    ui: {
                        dots: false,
                        rtl: false,
                        animation: false,
                        hover_mode: true,
                        theme_path: false,
                        theme_name: "umbraco"
                    },
                    langs: {
                        new_node: "New folder",
                        loading: "<div>" + (this._tree.settings.lang.loading || "Loading ...") + "</div>"
                    },
                    callback: {
                        //ensures that the node id isn't appended to the async url
                        beforedata: function(N, T) { return _this.onBeforeRequest(N, T); },
                        //wrapped functions maintain scope in callback
                        beforeopen: function(N, T) { _this.onBeforeOpen(N, T); },
                        onselect: function(N, T) { _this.onSelect(N, T); },
                        onchange: function(N, T) { _this.onChange(N, T); },
                        ondata: function(D, T) { return _this.onJSONData(D, T); },
                        onload: function(T) { if (initData == null) _this.onLoad(T); },
                        onparse: function(S, T) { return _this.onParse(S, T); },
                        error: function(E, T) { _this.onError(E, T); },
                        ondestroy: function(T) { _this.onDestroy(T); }
                    },
                    plugins: {
                        //UmbracoContext comes before context menu so that the events fire first
                        UmbracoContext: {
                            fullMenu: _this._opts.jsonFullMenu,
                            onBeforeContext: function(N, T, E) { return _this.onBeforeContext(N, T, E); }
                        },
                        contextmenu: {}
                    }
                };
                if (this._opts.treeMode != "standard") {
                    options.plugins.checkbox = { three_state: false }
                }

                //if there's no service URL, then disable ajax requests
                if (this._opts.serviceUrl == "" || this._opts.dataUrl == "") {
                    options.data.async = false;
                    options.data.opts.static = {};
                }

                //set global ajax settings:
                $.ajaxSetup({
                    contentType: "application/json; charset=utf-8"
                });

                this._debug("_getInitOptions. Async enabled = " + options.data.async);

                return options;
            }

        };
    }

    // instance manager
    Umbraco.Controls.UmbracoTree.cntr = 0;
    Umbraco.Controls.UmbracoTree.inst = {};


})(jQuery);
