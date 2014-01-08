/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference path="UmbracoUtils.js" />
/// <reference path="/umbraco_client/modal/modal.js" />
/// <reference path="language.aspx" />
/// <reference name="MicrosoftAjax.js"/>

Umbraco.Sys.registerNamespace("Umbraco.Application");

Umbraco.Application.Actions = function() {
    /// <summary>
    /// Application actions actions for the context menu, help dialogs, logout, etc...
    /// This class supports an event listener model. Currently the available events are:
    /// "nodeDeleting","nodeDeleted","nodeRefresh"
    /// </summary>

    return {
        _utils: Umbraco.Utils, //alias to Umbraco Utils
        _dialogWindow: null,
        /// <field name="_dialogWindow">A reference to a dialog window to open, any action that doesn't open in an overlay, opens in a dialog</field>
        _isDebug: false, //set to true to enable alert debugging
        _windowTitle: " - Umbraco CMS - ",
        _currApp: "",
        _isSaving: "",

        addEventHandler: function(fnName, fn) {
            /// <summary>Adds an event listener to the event name event</summary>
            if (typeof(jQuery) != "undefined") jQuery(window.top).bind(fnName, fn); //if there's no jQuery, there is no events
        },

        removeEventHandler: function(fnName, fn) {
            /// <summary>Removes an event listener to the event name event</summary>
            if (typeof(jQuery) != "undefined") jQuery(window.top).unbind(fnName, fn); //if there's no jQuery, there is no events
        },

        showSpeachBubble: function(ico, hdr, msg) {
            if (typeof(UmbClientMgr.mainWindow().UmbSpeechBubble) != "undefined") {
                UmbClientMgr.mainWindow().UmbSpeechBubble.ShowMessage(ico, hdr, msg);
            }
            else alert(msg);
        },

        launchHelp: function(lang, userType) {
            /// <summary>Launches the contextual help window</summary>
            var rightUrl = UmbClientMgr.contentFrame().document.location.href.split("\/");
            if (rightUrl.length > 0) {
                rightUrl = rightUrl[rightUrl.length - 1];
            }
            if (rightUrl.indexOf("?") > 0) {
                rightUrl = rightUrl.substring(0, rightUrl.indexOf("?"));
            }
            var url = "/umbraco/helpRedirect.aspx?Application=" + this._currApp + '&ApplicationURL=' + rightUrl + '&Language=' + lang + "&UserType=" + userType;
            window.open(url);
            return false;
        },

        launchAbout: function() {
            /// <summary>Launches the about Umbraco window</summary>
            UmbClientMgr.openModalWindow("dialogs/about.aspx", UmbClientMgr.uiKeys()['general_about'], true, 450, 390);
            return false;
        },

        launchCreateWizard: function() {
            /// <summary>Launches the create content wizard</summary>

            if (this._currApp == 'media' || this._currApp == 'content' || this._currApp == '') {
                if (this._currApp == '') {
                    this._currApp = 'content';
                }

                UmbClientMgr.openModalWindow("dialogs/create.aspx?nodeType=" + this._currApp + "&app=" + this._currApp + "&rnd=" + this._utils.generateRandom(), UmbClientMgr.uiKeys()['actions_create'] + " " + this._currApp, true, 620, 470);
                return false;

            }
            else
                alert('Not supported - please create by right clicking the parentnode and choose new...');
        },

        logout: function(t) {

            if (!t) {
                throw "The security token must be set in order to log a user out using this method";
            }

            if (confirm(UmbClientMgr.uiKeys()["defaultdialogs_confirmlogout"])) {
                //raise beforeLogout event
                jQuery(window.top).trigger("beforeLogout", []);

                document.location.href = 'logout.aspx?t=' + t;
            }
            return false;
        },

        submitDefaultWindow: function() {

            if (!this._isSaving) {
                this._isSaving = true;

                //v6 way
                var link = jQuery(".btn[id*=save]:first, .editorIcon[id*=save]:first, .editorIcon:input:image[id*=Save]:first");

                //this is made of bad, to work around webforms horrible wiring
                if(!link.hasClass("client-side") && link.attr("href").indexOf("javascript:") == 0){
                    eval(link.attr('href').replace('javascript:',''));
                }else{
                    link.click();
                }
            }
            this._isSaving = false;
            return false;
        },

        bindSaveShortCut: function () {
            
            var keys = "ctrl+s";
            if (navigator.platform.toUpperCase().indexOf('MAC') >= 0) {
                keys = "meta+s";
            }

            jQuery(document).bind('keydown', keys, function (evt) { UmbClientMgr.appActions().submitDefaultWindow(); return false; });
            jQuery(":input").bind('keydown', keys, function (evt) { UmbClientMgr.appActions().submitDefaultWindow(); return false; });
        },

        shiftApp: function (whichApp, appName) {
            /// <summary>Changes the application</summary>

            this._debug("shiftApp: " + whichApp + ", " + appName);

            UmbClientMgr.mainTree().saveTreeState(this._currApp == "" ? "content" : this._currApp);

            this._currApp = whichApp.toLowerCase();

            if (this._currApp != 'media' && this._currApp != 'content' && this._currApp != 'member') {
                jQuery("#buttonCreate").attr("disabled", "true").fadeOut(400);
                jQuery("#FindDocuments .umbracoSearchHolder").fadeOut(400);
            }
            else {
                // create button should still remain disabled for the memebers section
                if (this._currApp == 'member') {
                    jQuery("#buttonCreate").attr("disabled", "true").css("display", "inline-block").css("visibility", "hidden");
                }
                else {
                    jQuery("#buttonCreate").removeAttr("disabled").fadeIn(500).css("visibility", "visible");
                }
                jQuery("#FindDocuments .umbracoSearchHolder").fadeIn(500);
                //need to set the recycle bin node id based on app
                switch (this._currApp) {
                case ("media"):
                    UmbClientMgr.mainTree().setRecycleBinNodeId(-21);
                    break;
                case ("content"):
                    UmbClientMgr.mainTree().setRecycleBinNodeId(-20);
                    break;
                }
            }

            UmbClientMgr.mainTree().rebuildTree(whichApp, function(args) {
                //the callback will fire when the tree rebuilding is done, we
                //need to check the args to see if the tree was rebuild from cache
                //and if it had a previously selected node, if it didn't then load the dashboard.
                if (!args) {
                    UmbClientMgr.contentFrame('dashboard.aspx?app=' + whichApp);
                }
            });

            jQuery("#treeWindowLabel").html(appName);

            UmbClientMgr.mainWindow().document.title = appName + this._windowTitle + window.location.hostname.toLowerCase().replace('www', '');
        },

        getCurrApp: function() {
            return this._currApp;
        },


        //TODO: Move this into a window manager class
        openDialog: function(diaTitle, diaDoc, dwidth, dheight, optionalParams) {
            /// <summary>Opens the dialog window</summary>

            if (this._dialogWindow != null && !this._dialogWindow.closed) {
                this._dialogWindow.close();
            }
            this._dialogWindow = UmbClientMgr.mainWindow().open(diaDoc, 'dialogpage', "width=" + dwidth + "px,height=" + dheight + "px" + optionalParams);
        },

        openDashboard: function(whichApp) {
            UmbClientMgr.contentFrame('dashboard.aspx?app=' + whichApp);
        },

        actionTreeEditMode: function() {
            /// <summary></summary>
            UmbClientMgr.mainTree().toggleEditMode(true);
        },

        actionSort: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '0' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/sort.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&app=' + this._currApp + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sort'], true, 600, 450);
            }

        },

        actionChangeDocType: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '0' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/changeDocType.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&app=' + this._currApp + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_changeDocType'], true, 600, 600);
            }

        },

        actionRights: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/cruds.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_rights'], true, 800, 300);
            }
        },

        actionProtect: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/protectPage.aspx?mode=cut&nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_protect'], true, 535, 480);
            }
        },

        actionRollback: function() {
            /// <summary></summary>

            UmbClientMgr.openModalWindow('dialogs/rollback.aspx?nodeId=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_rollback'], true, 600, 550);
        },

        actionRefresh: function() {
            /// <summary></summary>

            //raise nodeRefresh event
            jQuery(window.top).trigger("nodeRefresh", []);
        },

        actionNotify: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/notifications.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_notify'], true, 300, 480);
            }
        },

        actionUpdate: function() {
            /// <summary></summary>
        },

        actionPublish: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '' != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/publish.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId, uiKeys['actions_publish'], true, 540, 280);
            }
        },

        actionToPublish: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                if (confirm(uiKeys['defaultdialogs_confirmSure'] + '\n\n')) {
                    UmbClientMgr.openModalWindow('dialogs/SendPublish.aspx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sendtopublish'], true, 300, 200);
                }
            }
        },

        actionQuit: function(t) {

            if (!t) {
                throw "The security token must be set in order to log a user out using this method";
            }

            if (confirm(uiKeys['defaultdialogs_confirmlogout'] + '\n\n'))
                document.location.href = 'logout.aspx?t=' + t;
        },

        actionRePublish: function() {
            /// <summary></summary>

            UmbClientMgr.openModalWindow('dialogs/republish.aspx?rnd=' + this._utils.generateRandom(), 'Republishing entire site', true, 450, 210);
        },

        actionAssignDomain: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/assignDomain2.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId, uiKeys['actions_assignDomain'], true, 500, 620);
            }
        },

        actionNew: function() {
            /// <summary>Show the create new modal overlay</summary>
            var actionNode = UmbClientMgr.mainTree().getActionNode();
            if (actionNode.nodeType != '') {
                if (actionNode.nodeType == "content") {
                    UmbClientMgr.openModalWindow("create.aspx?nodeId=" + actionNode.nodeId + "&nodeType=" + actionNode.nodeType + "&nodeName=" + actionNode.nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], true, 600, 425);
                }
                else if (actionNode.nodeType == "initmember") {
                    UmbClientMgr.openModalWindow("create.aspx?nodeId=" + actionNode.nodeId + "&nodeType=" + actionNode.nodeType + "&nodeName=" + actionNode.nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], true, 480, 380);
                }
                else if (actionNode.nodeType == "users") {
                    UmbClientMgr.openModalWindow("create.aspx?nodeId=" + actionNode.nodeId + "&nodeType=" + actionNode.nodeType + "&nodeName=" + actionNode.nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], true, 480, 380);
                }
                else if (actionNode.nodeType == "initpython" || actionNode.nodeType == "initdlrscripting") {
                    UmbClientMgr.openModalWindow("create.aspx?nodeId=" + actionNode.nodeId + "&nodeType=" + actionNode.nodeType + "&nodeName=" + actionNode.nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], true, 420, 380);
                }
                else {
                    UmbClientMgr.openModalWindow("create.aspx?nodeId=" + actionNode.nodeId + "&nodeType=" + actionNode.nodeType + "&nodeName=" + actionNode.nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], true, 420, 270);
                }
            }
        },

        actionNewFolder: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                this.openDialog("Opret", "createFolder.aspx?nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + "&nodeType=" + UmbClientMgr.mainTree().getActionNode().nodeType + "&nodeName=" + nodeName + '&rnd=' + this._utils.generateRandom(), 320, 225);
            }
        },

        actionSendToTranslate: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/sendToTranslation.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sendToTranslate'], true, 500, 470);
            }
        },

        actionEmptyTranscan: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/emptyTrashcan.aspx?type=" + this._currApp, uiKeys['actions_emptyTrashcan'], true, 500, 220);
            }
        },

        actionImport: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/importDocumentType.aspx?rnd=" + this._utils.generateRandom(), uiKeys['actions_importDocumentType'], true, 460, 400);
            }
        },

        actionExport: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                this.openDialog("Export", "dialogs/exportDocumentType.aspx?nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + "&rnd=" + this._utils.generateRandom(), 320, 205);
            }
        },

        actionAudit: function() {
            /// <summary></summary>

            UmbClientMgr.openModalWindow('dialogs/viewAuditTrail.aspx?nodeId=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_auditTrail'], true, 550, 500);
        },

        actionPackage: function() {
            /// <summary></summary>
        },

        actionDelete: function() {
            /// <summary></summary>

            var actionNode = UmbClientMgr.mainTree().getActionNode();
            if (UmbClientMgr.mainTree().getActionNode().nodeType == "content" && UmbClientMgr.mainTree().getActionNode().nodeId == '-1')
                return;

            this._debug("actionDelete");

            // tg: quick workaround for the are you sure you want to delete 'null' confirm message happening when deleting xslt files
            currrentNodeName = UmbClientMgr.mainTree().getActionNode().nodeName;
            if (currrentNodeName == null || currrentNodeName == "null") {
                currrentNodeName = UmbClientMgr.mainTree().getActionNode().nodeId;
            }

            if (confirm(uiKeys['defaultdialogs_confirmdelete'] + ' "' + currrentNodeName + '"?\n\n')) {
                //raise nodeDeleting event
                jQuery(window.top).trigger("nodeDeleting", []);
                var _this = this;

                //check if it's in the recycle bin
                if (actionNode.jsNode.closest("li[id='-20']").length == 1 || actionNode.jsNode.closest("li[id='-21']").length == 1) {
                    umbraco.presentation.webservices.legacyAjaxCalls.DeleteContentPermanently(
                        UmbClientMgr.mainTree().getActionNode().nodeId,
                        UmbClientMgr.mainTree().getActionNode().nodeType,
                        function() {
                            _this._debug("actionDelete: Raising event");
                            //raise nodeDeleted event
                            jQuery(window.top).trigger("nodeDeleted", []);
                        });
                }
                else {
                    umbraco.presentation.webservices.legacyAjaxCalls.Delete(
                        UmbClientMgr.mainTree().getActionNode().nodeId,
                        UmbClientMgr.mainTree().getActionNode().nodeName,
                        UmbClientMgr.mainTree().getActionNode().nodeType,
                        function() {
                            _this._debug("actionDelete: Raising event");
                            //raise nodeDeleted event
                            jQuery(window.top).trigger("nodeDeleted", []);
                        },
                        function(error) {
                            _this._debug("actionDelete: Raising public error event");
                            //raise public error event
                            jQuery(window.top).trigger("publicError", [error]);
                        })
                }
            }

        },

        actionDisable: function() {
            /// <summary>
            /// Used for users when disable is selected.
            /// </summary>

            if (confirm(uiKeys['defaultdialogs_confirmdisable'] + ' "' + UmbClientMgr.mainTree().getActionNode().nodeName + '"?\n\n')) {
                umbraco.presentation.webservices.legacyAjaxCalls.DisableUser(UmbClientMgr.mainTree().getActionNode().nodeId, function() {
                    UmbClientMgr.mainTree().reloadActionNode();
                });
            }
        },

        actionMove: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/moveOrCopy.aspx?app=" + this._currApp + "&mode=cut&id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_move'], true, 500, 460);
            }
        },

        actionCopy: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.openModalWindow("dialogs/moveOrCopy.aspx?app=" + this._currApp + "&mode=copy&id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_copy'], true, 500, 470);
            }
        },
        _debug: function(strMsg) {
            if (this._isDebug) {
                Sys.Debug.trace("AppActions: " + strMsg);
            }
        }
    };
};
