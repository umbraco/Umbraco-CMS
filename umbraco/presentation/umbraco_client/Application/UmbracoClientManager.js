/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
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
            _debug: false,
            _mainTree: null,
            _appActions: null,
            _rootPath: "/umbraco", //this is the default

            setUmbracoPath: function(strPath) {
                /// <summary>
                /// sets the Umbraco root path folder
                /// </summary>
                _rootPath = strPath;
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

                        this._mainTree = $("<div id='falseTree' />").appendTo("body").hide().UmbracoTree({
                            uiKeys: this.uiKeys(),
                            jsonFullMenu: {},
                            jsonInitNode: {},
                            appActions: this.appActions()
                        }).UmbracoTreeAPI();
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
            contentFrame: function(strLocation) {
                /// <summary>
                /// This will return the reference to the right content frame if strLocation is null or empty,
                /// or set the right content frames location to the one specified by strLocation.
                /// </summary>

                //TODO: If there is no main window, we need to return the current window!
                this._debug("contentFrame: " + strLocation);
                if (strLocation == null || strLocation == "") {
                    return this.mainWindow().right;
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

                    this.mainWindow().right.location.href = strLocation;
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
            },
            setUp: function() {
                console.log(this.mainTree().find('a'));
            }
        }
    }
})(jQuery);

//define alias for use throughout application
var UmbClientMgr = new Umbraco.Application.ClientManager();
UmbClientMgr.setUp();