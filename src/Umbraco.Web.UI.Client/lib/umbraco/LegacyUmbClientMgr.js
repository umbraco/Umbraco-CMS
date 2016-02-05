
//TODO: WE NEED TO CONVERT ALL OF THESE METHODS TO PROXY TO OUR APPLICATION SINCE MANY CUSTOM APPS USE THIS!

//TEST to mock iframe, this intercepts calls directly
//to the old iframe, and funnels requests to angular directly
//var right = {document: {location: {}}};
/**/

Umbraco.Sys.registerNamespace("Umbraco.Application");

(function($) {
    Umbraco.Application.ClientManager = function() {

        //to support those trying to call right.document.etc
        var fakeFrame  = {};
        Object.defineProperty(fakeFrame, "href", {
            get: function() {
                return this._href ? this._href : "";
            },
            set: function(value) {
                this._href = value;
                UmbClientMgr.contentFrame(value);
            },
        });

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
            mainTree: function () {

                var injector = getRootInjector();
                var navService = injector.get("navigationService");
                var appState = injector.get("appState");
                var angularHelper = injector.get("angularHelper");
                var $rootScope = injector.get("$rootScope");

                //mimic the API of the legacy tree
                var tree = {
                    setActiveTreeType: function (treeType) {
                        angularHelper.safeApply($rootScope, function() {
                            navService._setActiveTreeType(treeType);
                        });
                    },
                    syncTree: function (path, forceReload) {
                        angularHelper.safeApply($rootScope, function() {
                            navService._syncPath(path, forceReload);
                        });
                    },
                    clearTreeCache: function(){
                        var treeService = injector.get("treeService");
                        angularHelper.safeApply($rootScope, function() {
                            treeService.clearCache();
                        });
                    },
                    reloadActionNode: function () {
                        angularHelper.safeApply($rootScope, function() {
                            var currentMenuNode = appState.getMenuState("currentNode");
                            if (currentMenuNode) {
                                navService.reloadNode(currentMenuNode);
                            }
                        });
                    },
                    refreshTree: function (treeAlias) {
                        angularHelper.safeApply($rootScope, function() {
                            navService._setActiveTreeType(treeAlias, true);
                        });                        
                    },
                    moveNode: function (id, path) {
                        angularHelper.safeApply($rootScope, function() {
                            var currentMenuNode = appState.getMenuState("currentNode");
                            if (currentMenuNode) {
                                var treeService = injector.get("treeService");
                                var treeRoot = treeService.getTreeRoot(currentMenuNode);
                                if (treeRoot) {
                                    var found = treeService.getDescendantNode(treeRoot, id);
                                    if (found) {
                                        treeService.removeNode(found);
                                    }
                                }
                            }
                            navService._syncPath(path, true);
                        });                        
                    },
                    getActionNode: function () {
                        //need to replicate the legacy tree node
                        var currentMenuNode = appState.getMenuState("currentNode");
                        if (!currentMenuNode) {
                            return null;
                        }
                        
                        var legacyNode = {
                            nodeId: currentMenuNode.id,
                            nodeName: currentMenuNode.name,
                            nodeType: currentMenuNode.nodeType,
                            treeType: currentMenuNode.nodeType,
                            sourceUrl: currentMenuNode.childNodesUrl,
                            updateDefinition: function() {
                                throw "'updateDefinition' method is not supported in Umbraco 7, consider upgrading to the new v7 APIs";
                            }
                        };
                        //defined getters that will throw a not implemented/supported exception
                        Object.defineProperty(legacyNode, "menu", {
                            get: function () {
                                throw "'menu' property is not supported in Umbraco 7, consider upgrading to the new v7 APIs";
                            }
                        });
                        Object.defineProperty(legacyNode, "jsNode", {
                            get: function () {
                                throw "'jsNode' property is not supported in Umbraco 7, consider upgrading to the new v7 APIs";
                            }
                        });
                        Object.defineProperty(legacyNode, "jsTree", {
                            get: function () {
                                throw "'jsTree' property is not supported in Umbraco 7, consider upgrading to the new v7 APIs";
                            }
                        });

                        return legacyNode;
                    }
                };

                return tree;
            },
            appActions: function() {
                var injector = getRootInjector();
                var navService = injector.get("navigationService");
                var localizationService = injector.get("localizationService");
                var userResource = injector.get("userResource");                
                //var appState = injector.get("appState");
                var angularHelper = injector.get("angularHelper");
                var $rootScope = injector.get("$rootScope");
                
                var actions = {
                    openDashboard : function(section){
                        navService.changeSection(section);
                    },
                    actionDisable: function () {
                        localizationService.localize("defaultdialogs_confirmdisable").then(function (txtConfirmDisable) {
                            var currentMenuNode = UmbClientMgr.mainTree().getActionNode();
                            if (currentMenuNode) {
                                if (confirm(txtConfirmDisable + ' "' + UmbClientMgr.mainTree().getActionNode().nodeName + '"?\n\n')) {
                                    angularHelper.safeApply($rootScope, function () {
                                        userResource.disableUser(currentMenuNode.nodeId).then(function () {
                                            UmbClientMgr.mainTree().syncTree("-1," + currentMenuNode.nodeId, true);
                                        });
                                    });
                                }
                            }
                        });
                    }
                };

                return actions;                
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

                    if (typeof top.right != "undefined") {
                        return top.right;
                    }
                    else {
                        return top; //return the current window if the content frame doesn't exist in the current context
                    }
                    //return;
                }

                this._debug("contentFrame: " + strLocation);

                //get our angular navigation service
                var injector = getRootInjector();

                var rootScope = injector.get("$rootScope");
                var angularHelper = injector.get("angularHelper");
                var navService = injector.get("navigationService");
                var locationService = injector.get("$location");

                var self = this;

                angularHelper.safeApply(rootScope, function() {
                    if (strLocation.startsWith("#")) {
                        locationService.path(strLocation.trimStart("#")).search("");
                    }
                    else {
                        //if the path doesn't start with "/" or with the root path then 
                        //prepend the root path
                        if (!strLocation.startsWith("/")) {
                            strLocation = self._rootPath + "/" + strLocation;
                        }
                        else if (strLocation.length >= self._rootPath.length
                            && strLocation.substr(0, self._rootPath.length) != self._rootPath) {
                            strLocation = self._rootPath + "/" + strLocation;
                        }

                        navService.loadLegacyIFrame(strLocation);
                    }
                });
            },
            
            getFakeFrame : function() {
                return fakeFrame;
            },

            /** This is used to launch an angular based modal window instead of the legacy window */
            openAngularModalWindow: function (options) {

                //get our angular navigation service
                var injector = getRootInjector();
                var dialogService = injector.get("dialogService");

                var dialog = dialogService.open(options);

                ////add the callback to the jquery data for the modal so we can call it on close to support the legacy way dialogs worked.
                //dialog.element.data("modalCb", onCloseCallback);
               
                //this._modal.push(dialog);
                //return dialog;
            },

            openModalWindow: function(url, name, showHeader, width, height, top, leftOffset, closeTriggers, onCloseCallback) {
                //need to create the modal on the top window if the top window has a client manager, if not, create it on the current window                
                
                //get our angular navigation service
                var injector = getRootInjector();
                var navService = injector.get("navigationService");
                var dialogService = injector.get("dialogService"); 

                var self = this;

                //based on what state the nav ui is in, depends on how we are going to launch a model / dialog. A modal
                // will show up on the right hand side and a dialog will show up as if it is in the menu.
                // with the legacy API we cannot know what is expected so we can only check if the menu is active, if it is
                // we'll launch a dialog, otherwise a modal.
                var appState = injector.get("appState");
                var navMode = appState.getGlobalState("navMode");
                var dialog;
                if (navMode === "menu") {
                    dialog = navService.showDialog({
                        //create a 'fake' action to passin with the specified actionUrl since it needs to load into an iframe
                        action: {
                            name: name,
                            metaData: {
                                actionUrl: url
                            }
                        },
                        node: {}
                    });
                }
                else {
                    dialog = dialogService.open({
                        template: url,
                        width: width,
                        height: height,
                        iframe: true,
                        show: true
                    });
                }
                

                //add the callback to the jquery data for the modal so we can call it on close to support the legacy way dialogs worked.
                dialog.element.data("modalCb", onCloseCallback);
                //add the close triggers
                if (angular.isArray(closeTriggers)) {
                    for (var i = 0; i < closeTriggers.length; i++) {
                        var e = dialog.find(closeTriggers[i]);
                        if (e.length > 0) {
                            e.click(function () {
                                self.closeModalWindow();
                            });
                        }
                    }
                }

                this._modal.push(dialog);
                return dialog;
            },
            rootScope : function(){
                return getRootScope();
            },

            reloadLocation: function() {
                var injector = getRootInjector();
                var $route = injector.get("$route");
                $route.reload();
            },
            
            closeModalWindow: function(rVal) {
                
                //get our angular navigation service
                var injector = getRootInjector();
                var dialogService = injector.get("dialogService");

                // all legacy calls to closeModalWindow are expecting to just close the last opened one so we'll ensure
                // that this is still the case.
                if (this._modal != null && this._modal.length > 0) {

                    var lastModal = this._modal.pop();

                    //if we've stored a callback on this modal call it before we close.
                    var self = this;
                    //get the compat callback from the modal element
                    var onCloseCallback = lastModal.element.data("modalCb");
                    if (typeof onCloseCallback == "function") {
                        onCloseCallback.apply(self, [{ outVal: rVal }]);
                    }

                    //just call the native dialog close() method to remove the dialog
                    lastModal.close();

                    //if it's the last one close them all
                    if (this._modal.length == 0) {
                        getRootScope().$emit("app.closeDialogs", undefined);
                    }
                }
                else {
                    //instead of calling just the dialog service we funnel it through the global 
                    //event emitter
                    getRootScope().$emit("app.closeDialogs", undefined);
                }                
            },
            /* This is used for the package installer to call in order to reload all app assets so we don't have to reload the window */
            _packageInstalled: function() {
                var injector = getRootInjector();
                var packageHelper = injector.get("packageHelper");
                packageHelper.packageInstalled();
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
