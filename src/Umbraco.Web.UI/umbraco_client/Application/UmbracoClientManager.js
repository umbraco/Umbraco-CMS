﻿/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference path="/umbraco_client/Application/HistoryManager.js" />
/// <reference path="/umbraco_client/ui/jquery.js" />
/// <reference path="/umbraco_client/Tree/UmbracoTree.js" />
/// <reference name="MicrosoftAjax.js"/>

Umbraco.Sys.registerNamespace("Umbraco.Application");

(function($) {
    Umbraco.Application.ClientManager = function() {
        /// <summary>
        /// A class which ensures that all calls made to the objects that it owns are done in the context
        /// of the main Umbraco application window.
        /// </summary>

        return {
            _isDirty: false,
            _isDebug: false,
            _mainTree: null,
            _appActions: null,
            _historyMgr: null,
            _rootPath: "/umbraco", //this is the default
            _modal: new Array(), //track all modal window objects (they get stacked)

            historyManager: function() {
                if (!this._historyMgr) {
                    this._historyMgr = new Umbraco.Controls.HistoryManager();
                }
                return this._historyMgr;
            },

            setUmbracoPath: function(strPath) {
                /// <summary>
                /// sets the Umbraco root path folder
                /// </summary>
                this._debug("setUmbracoPath: " + strPath);
                this._rootPath = strPath;
            },

            mainWindow: function() {
                /// <summary>
                /// Returns a reference to the main frame of the application
                /// </summary>
                return top;
            },
            mainTree: function() {
                /// <summary>
                /// Returns a reference to the main UmbracoTree API object.
                /// Sometimes an Umbraco page will need to be opened without being contained in the iFrame from the main window
                /// so this method is will construct a false tree to be returned if this is the case as to avoid errors.
                /// </summary>
                /// <returns type="Umbraco.Controls.UmbracoTree" />

                if (this._mainTree == null) {
                    if (this.mainWindow().jQuery == null
                        || this.mainWindow().jQuery(".umbTree").length == 0
                        || this.mainWindow().jQuery(".umbTree").UmbracoTreeAPI() == null) {
                        //creates a false tree with all the public tree params set to a false method.
                        var tmpTree = {};
                        var treeProps = ["init", "setRecycleBinNodeId", "clearTreeCache", "toggleEditMode", "refreshTree", "rebuildTree", "saveTreeState", "syncTree", "childNodeCreated", "moveNode", "copyNode", "findNode", "selectNode", "reloadActionNode", "getActionNode", "setActiveTreeType", "getNodeDef"];
                        for (var p in treeProps) {
                            tmpTree[treeProps[p]] = function() { return false; };
                        }
                        this._mainTree = tmpTree;
                    }
                    else {
                        this._mainTree = this.mainWindow().jQuery(".umbTree").UmbracoTreeAPI();
                    }
                }
                return this._mainTree;
            },
            appActions: function() {
                /// <summary>
                /// Returns a reference to the application actions object
                /// </summary>

                //if the main window has no actions, we'll create some
                if (this._appActions == null) {
                    if (typeof this.mainWindow().appActions == 'undefined') {
                        this._appActions = new Umbraco.Application.Actions();
                    }
                    else this._appActions = this.mainWindow().appActions;
                }
                return this._appActions;
            },
            uiKeys: function() {
                /// <summary>
                /// Returns a reference to the main windows uiKeys object for globalization
                /// </summary>

                //TODO: If there is no main window, we need to go retrieve the appActions from the server!
                return this.mainWindow().uiKeys;
            },
            //    windowMgr: function() 
            //        return null;
            //    },
            contentFrameAndSection: function(app, rightFrameUrl){
                //this.appActions().shiftApp(app, this.uiKeys()['sections_' + app]);
                var self = this;
                self.mainWindow().UmbClientMgr.historyManager().addHistory(app,true);
                window.setTimeout(function(){
                    self.mainWindow().UmbClientMgr.contentFrame(rightFrameUrl);
                },200);
            },
            contentFrame: function(strLocation) {
                /// <summary>
                /// This will return the reference to the right content frame if strLocation is null or empty,
                /// or set the right content frames location to the one specified by strLocation.
                /// </summary>

                this._debug("contentFrame: " + strLocation);

                if (strLocation == null || strLocation == "") {
                    if (typeof this.mainWindow().right != "undefined") {
                        return this.mainWindow().right;
                    }
                    else {
                        return this.mainWindow(); //return the current window if the content frame doesn't exist in the current context
                    }
                }
                else {
                    //if the path doesn't start with "/" or with the root path then 
                    //prepend the root path
                    if (strLocation.substr(0, 1) != "/") {
                        strLocation = this._rootPath + "/" + strLocation;
                    }
                    else if (strLocation.length >= this._rootPath.length
                        && strLocation.substr(0, this._rootPath.length) != this._rootPath) {
                        strLocation = this._rootPath + "/" + strLocation;
                    }

                    this._debug("contentFrame: parsed location: " + strLocation);
                    var self = this;
                    window.setTimeout(function(){
                        if (typeof self.mainWindow().right != "undefined") {
                            self.mainWindow().right.location.href = strLocation;
                        }
                        else {
                            self.mainWindow().location.href = strLocation; //set the current windows location if the right frame doesn't exist int he current context
                        }
                    },200);
                }
            },
            reloadContentFrameUrlIfPathLoaded: function (url) {
                var contentFrame;
                if (typeof this.mainWindow().right != "undefined") {
                    contentFrame = this.mainWindow().right;
                }
                else {
                    contentFrame = this.mainWindow(); 
                }

                var currentPath = contentFrame.location.pathname + (contentFrame.location.search ? contentFrame.location.search : "");
                if (currentPath == url) {
                    contentFrame.location.reload();
                }
            },
            openModalWindow: function(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback) {
                //need to create the modal on the top window if the top window has a client manager, if not, create it on the current window                

                //if this is the top window, or if the top window doesn't have a client manager, create the modal in this manager
                if (window == this.mainWindow() || !this.mainWindow().UmbClientMgr) {
                    var m = new Umbraco.Controls.ModalWindow();
                    this._modal.push(m);
                    m.open(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback);
                }
                else {
                    //if the main window has a client manager, then call the main window's open modal method whilst keeping the context of it's manager.
                    if (this.mainWindow().UmbClientMgr) {
                        this.mainWindow().UmbClientMgr.openModalWindow.apply(this.mainWindow().UmbClientMgr,
                            [url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback]);
                    }
                    else {
                        return; //exit recurse.
                    }
                }
            },
            closeModalWindow: function(rVal) {
                /// <summary>
                /// will close the latest open modal window.
                /// if an rVal is passed in, then this will be sent to the onCloseCallback method if it was specified.
                /// </summary>
                if (this._modal != null && this._modal.length > 0) {
                    this._modal.pop().close(rVal);
                }
                else {
                    //this will recursively try to close a modal window until the parent window has a modal object or the window is the top and has the modal object
                    var mgr = null;
                    if (window.parent == null || window.parent == window) {
                        //we are at the root window, check if we can close the modal window from here
                        if (window.UmbClientMgr != null && window.UmbClientMgr._modal != null && window.UmbClientMgr._modal.length > 0) {
                            mgr = window.UmbClientMgr;
                        }
                        else {
                            return; //exit recursion.
                        }
                    }
                    else if (typeof window.parent.UmbClientMgr != "undefined") {
                        mgr = window.parent.UmbClientMgr;
                    }
                    mgr.closeModalWindow.call(mgr, rVal);
                }
            },
            _debug: function(strMsg) {
                if (this._isDebug) {
                    Sys.Debug.trace("UmbClientMgr: " + strMsg);
                }
            },
            get_isDirty: function() {
                return this._isDirty;
            },
            set_isDirty: function(value) {
                this._isDirty = value;
            }
        }
    }
})(jQuery);

//define alias for use throughout application
var UmbClientMgr = new Umbraco.Application.ClientManager();
