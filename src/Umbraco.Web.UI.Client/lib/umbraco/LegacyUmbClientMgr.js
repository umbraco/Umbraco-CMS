
//TODO: WE NEED TO CONVERT ALL OF THESE METHODS TO PROXY TO OUR APPLICATION SINCE MANY CUSTOM APPS USE THIS!

//TEST to mock iframe, this intercepts calls directly
//to the old iframe, and funnels requests to angular directly
//var right = {document: {location: {}}};
/*Object.defineProperty(right.document.location, "href", {
    get: function() {
        return this._href ? this._href : "";
    },
    set: function(value) {
        this._href = value;
        UmbClientMgr.contentFrame(value);
    },
});*/

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
                var injector = getRootInjector();
                var navService = injector.get("navigationService");

                var tree = {
                    setActiveTreeType : function(treeType){
                         navService.syncTree(null, treeType, null);
                    },
                    syncTree : function(path,forceReload){
                        navService.syncPath(path);
                    }
                };

                return tree;
            },
            appActions: function() {
                var injector = getRootInjector();
                var navService = injector.get("navigationService");

                var _actions = {};
                _actions.openDashboard = function(section){
                    navService.changeSection(section);
                };

                return _actions;
                //throw "Not implemented!";

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
                var dialogService = injector.get("dialogService");

                var self = this;

                var dialog = dialogService.open({
                    template: url,
                    width: width,
                    height: height,
                    iframe: true,
                    show: true
                });

                //add the callback to the jquery data for the modal so we can call it on close to support the legacy way dialogs worked.
                dialog.element.data("modalCb", onCloseCallback);
                //add the close triggers
                for (var i = 0; i < closeTriggers.length; i++) {
                    var e = dialog.find(closeTriggers[i]);
                    if (e.length > 0) {
                        e.click(function() {
                            self.closeModalWindow();
                        });
                    }
                }

                this._modal.push(dialog);
                return dialog;
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
                }
                else {
                    //instead of calling just the dialog service we funnel it through the global 
                    //event emitter
                    getRootScope().$emit("closeDialogs", event);
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
        };
    };
})(jQuery);

//define alias for use throughout application
var UmbClientMgr = new Umbraco.Application.ClientManager();
