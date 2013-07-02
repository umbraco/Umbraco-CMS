
//TODO: WE NEED TO CONVERT ALL OF THESE METHODS TO PROXY TO OUR APPLICATION SINCE MANY CUSTOM APPS USE THIS!

Umbraco.Sys.registerNamespace("Umbraco.Application");

(function($) {
    Umbraco.Application.ClientManager = function() {

        /**
         * @ngdoc function
         * @name getRootScope
         * @methodOf UmbClientMgr
         * @function
         *
         * @description
         * Returns the root angular scope
         */
        function getRootScope() {
            return angular.element(document.getElementById("umbracoMainPageBody")).scope();
        }
        
        /**
         * @ngdoc function
         * @name getRootInjector
         * @methodOf UmbClientMgr
         * @function
         *
         * @description
         * Returns the root angular injector
         */
        function getRootInjector() {
            return angular.element(document.getElementById("umbracoMainPageBody")).injector();
        }


        return {
            _isDirty: false,
            _isDebug: false,
            _mainTree: null,
            _appActions: null,
            _historyMgr: null,
            _rootPath: "/umbraco", //this is the default
            _modal: new Array(), //track all modal window objects (they get stacked)

            historyManager: function () {
                
                throw "Not implemented!";

                //if (!this._historyMgr) {
                //    this._historyMgr = new Umbraco.Controls.HistoryManager();
                //}
                //return this._historyMgr;
            },

            setUmbracoPath: function(strPath) {
                /// <summary>
                /// sets the Umbraco root path folder
                /// </summary>
                this._debug("setUmbracoPath: " + strPath);
                this._rootPath = strPath;
            },

            mainWindow: function() {
                return top;
            },
            mainTree: function() {
              
                throw "Not implemented!";

                //if (this._mainTree == null) {
                //    if (this.mainWindow().jQuery == null
                //        || this.mainWindow().jQuery(".umbTree").length == 0
                //        || this.mainWindow().jQuery(".umbTree").UmbracoTreeAPI() == null) {
                //        //creates a false tree with all the public tree params set to a false method.
                //        var tmpTree = {};
                //        var treeProps = ["init", "setRecycleBinNodeId", "clearTreeCache", "toggleEditMode", "refreshTree", "rebuildTree", "saveTreeState", "syncTree", "childNodeCreated", "moveNode", "copyNode", "findNode", "selectNode", "reloadActionNode", "getActionNode", "setActiveTreeType", "getNodeDef"];
                //        for (var p in treeProps) {
                //            tmpTree[treeProps[p]] = function() { return false; };
                //        }
                //        this._mainTree = tmpTree;
                //    }
                //    else {
                //        this._mainTree = this.mainWindow().jQuery(".umbTree").UmbracoTreeAPI();
                //    }
                //}
                //return this._mainTree;
            },
            appActions: function() {
               
                throw "Not implemented!";

                ////if the main window has no actions, we'll create some
                //if (this._appActions == null) {
                //    if (typeof this.mainWindow().appActions == 'undefined') {
                //        this._appActions = new Umbraco.Application.Actions();
                //    }
                //    else this._appActions = this.mainWindow().appActions;
                //}
                //return this._appActions;
            },
            uiKeys: function() {
                
                throw "Not implemented!";
                
                ////TODO: If there is no main window, we need to go retrieve the appActions from the server!
                //return this.mainWindow().uiKeys;
            },
            contentFrameAndSection: function(app, rightFrameUrl) {

                throw "Not implemented!";

                ////this.appActions().shiftApp(app, this.uiKeys()['sections_' + app]);
                //var self = this;
                //self.mainWindow().UmbClientMgr.historyManager().addHistory(app, true);
                //window.setTimeout(function() {
                //    self.mainWindow().UmbClientMgr.contentFrame(rightFrameUrl);
                //}, 200);
            },

            /**
             * @ngdoc function
             * @name contentFrame
             * @methodOf UmbClientMgr
             * @function
             *
             * @description
             * This will tell our angular app to create and load in an iframe at the specified location
             * @param strLocation {String} The URL to load the iframe in
             */
            contentFrame: function (strLocation) {
                
                if (!strLocation || strLocation == "") {
                    //SD: NOTE: We used to return the content iframe object but now I'm not sure we should do that ?!
                    return;
                }

                this._debug("contentFrame: " + strLocation);

                //get our angular navigation service
                var injector = getRootInjector();
                var navService = injector.get("navigationService");

                //if the path doesn't start with "/" or with the root path then 
                //prepend the root path
                if (!strLocation.startsWith("/")) {
                    strLocation = this._rootPath + "/" + strLocation;
                }
                else if (strLocation.length >= this._rootPath.length
                    && strLocation.substr(0, this._rootPath.length) != this._rootPath) {
                    strLocation = this._rootPath + "/" + strLocation;
                }

                navService.loadLegacyIFrame(strLocation);

            },
            openModalWindow: function(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback) {
                //need to create the modal on the top window if the top window has a client manager, if not, create it on the current window                
                
                //get our angular navigation service
                var injector = getRootInjector();
                var dialogService = injector.get("dialogService");
                
                var dialog = dialogService.open({
                    template: url,
                    width: width,
                    height: height,
                    iframe: true,
                    show: true,
                    callback: function (result) {
                        dialog.hide();
                    }
                });

               // var m = new Umbraco.Controls.ModalWindow();
               // this._modal.push(m);
               // m.open(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback);

                //throw "Not implemented!";

                ////if this is the top window, or if the top window doesn't have a client manager, create the modal in this manager
                //if (window == this.mainWindow() || !this.mainWindow().UmbClientMgr) {
                //    var m = new Umbraco.Controls.ModalWindow();
                //    this._modal.push(m);
                //    m.open(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback);
                //}
                //else {
                //    //if the main window has a client manager, then call the main window's open modal method whilst keeping the context of it's manager.
                //    if (this.mainWindow().UmbClientMgr) {
                //        this.mainWindow().UmbClientMgr.openModalWindow.apply(this.mainWindow().UmbClientMgr,
                //            [url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback]);
                //    }
                //    else {
                //        return; //exit recurse.
                //    }
                //}
            },
            closeModalWindow: function(rVal) {
                
                throw "Not implemented!";

                //if (this._modal != null && this._modal.length > 0) {
                //    this._modal.pop().close(rVal);
                //}
                //else {
                //    //this will recursively try to close a modal window until the parent window has a modal object or the window is the top and has the modal object
                //    var mgr = null;
                //    if (window.parent == null || window.parent == window) {
                //        //we are at the root window, check if we can close the modal window from here
                //        if (window.UmbClientMgr != null && window.UmbClientMgr._modal != null && window.UmbClientMgr._modal.length > 0) {
                //            mgr = window.UmbClientMgr;
                //        }
                //        else {
                //            return; //exit recursion.
                //        }
                //    }
                //    else if (typeof window.parent.UmbClientMgr != "undefined") {
                //        mgr = window.parent.UmbClientMgr;
                //    }
                //    mgr.closeModalWindow.call(mgr, rVal);
                //}
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
        };
    };
})(jQuery);

//define alias for use throughout application
var UmbClientMgr = new Umbraco.Application.ClientManager();
