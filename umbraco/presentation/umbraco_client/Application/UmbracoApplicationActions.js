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
    /// "nodeDeleting","nodeDeleted","nodeRefresh","beforeLogout"
    /// </summary>

    return {

        _utils: Umbraco.Utils, //alias to Umbraco Utils
        _dialogWindow: null,
        /// <field name="_dialogWindow">A reference to a dialog window to open, any action that doesn't open in an overlay, opens in a dialog</field>
        _isDebug: false, //set to true to enable alert debugging
        _windowTitle: " - Umbraco CMS - ",
        _currApp: "",

        addEventHandler: function(fnName, fn) {
            /// <summary>Adds an event listener to the event name event</summary>
            if (typeof (jQuery) != "undefined") jQuery(this).bind(fnName, fn); //if there's no jQuery, there is no events
        },

        removeEventHandler: function(fnName, fn) {
            /// <summary>Removes an event listener to the event name event</summary>
            if (typeof (jQuery) != "undefined") jQuery(this).unbind(fnName, fn); //if there's no jQuery, there is no events
        },

        showSpeachBubble: function(ico, hdr, msg) {
            if (typeof (UmbClientMgr.mainWindow().UmbSpeechBubble) != "undefined") {
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
            var url = "http://umbraco.org/redir/help/" + lang + "/" + userType + "/" + this._currApp + "/" + rightUrl;
            window.open(url, 'help', 'width=750,height=500,scrollbars=auto,resizable=1;');
            return false;
        },

        launchAbout: function() {
            /// <summary>Launches the about Umbraco window</summary>
            UmbClientMgr.mainWindow().openModal("dialogs/about.aspx", UmbClientMgr.uiKeys()['general_about'], 390, 450);
            return false;
        },

        launchCreateWizard: function() {
            /// <summary>Launches the create content wizard</summary>

            if (this._currApp == 'media' || this._currApp == 'content' || this._currApp == '') {
                if (this._currApp == '') {
                    this._currApp = 'content';
                }

                UmbClientMgr.mainWindow().openModal("dialogs/create.aspx?nodeType=" + this._currApp + "&app=" + this._currApp + "&rnd=" + this._utils.generateRandom(), UmbClientMgr.uiKeys()['actions_create'] + " " + this._currApp, 470, 620);
                return false;

            } else
                alert('Not supported - please create by right clicking the parentnode and choose new...');
        },

        logout: function() {
            /// <summary>Logs the user out</summary>
            if (confirm(UmbClientMgr.uiKeys()["defaultdialogs_confirmlogout"])) {
                //raise beforeLogout event
                jQuery(this).trigger("beforeLogout", []);

                document.location.href = 'logout.aspx';
            }
            return false;
        },

        shiftApp: function(whichApp, appName, ignoreDashboard) {
            /// <summary>Changes the application</summary>

            this._debug("shiftApp: " + whichApp + ", " + appName + ", " + ignoreDashboard);


            UmbClientMgr.mainTree().saveTreeState(this._currApp == "" ? "content" : this._currApp);

            this._currApp = whichApp.toLowerCase();

            if (this._currApp != 'media' && this._currApp != 'content') {
                jQuery("#buttonCreate").attr("disabled", "true");
            }
            else {
                jQuery("#buttonCreate").removeAttr("disabled");
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

            if (!ignoreDashboard) {
                UmbClientMgr.contentFrame('dashboard.aspx?app=' + whichApp);
            }



            UmbClientMgr.mainTree().rebuildTree(whichApp);

            jQuery("#treeWindowLabel").html(appName);

            UmbClientMgr.mainWindow().document.title = appName + this._windowTitle + window.location.hostname.toLowerCase().replace('www', '');

            //TODO: Update this to use microsoft's history manager
            UmbClientMgr.mainWindow().location.hash = whichApp;
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
                UmbClientMgr.mainWindow().openModal("dialogs/sort.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sort'], 450, 600);
            }

        },

        actionRights: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/cruds.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_rights'], 300, 800);
            }
        },

        actionProtect: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/protectPage.aspx?mode=cut&nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_protect'], 480, 535);
            }
        },

        actionRollback: function() {
            /// <summary></summary>

            UmbClientMgr.mainWindow().openModal('dialogs/rollback.aspx?nodeId=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_rollback'], 550, 600);
        },

        actionRefresh: function() {
            /// <summary></summary>

            //raise nodeRefresh event
            jQuery(this).trigger("nodeRefresh", []);
        },

        actionNotify: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/notifications.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_notify'], 480, 300);
            }
        },

        actionUpdate: function() {
            /// <summary></summary>
        },

        actionPublish: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '' != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/publish.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId, uiKeys['actions_publish'], 280, 540);
            }
        },

        actionToPublish: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                if (confirm(uiKeys['defaultdialogs_confirmSure'] + '\n\n')) {
                    UmbClientMgr.mainWindow().openModal('dialogs/SendPublish.aspx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sendtopublish'], 200, 300);
                }
            }
        },

        actionQuit: function() {
            /// <summary></summary>

            if (confirm(uiKeys['defaultdialogs_confirmlogout'] + '\n\n'))
                document.location.href = 'logout.aspx';
        },

        actionRePublish: function() {
            /// <summary></summary>

            UmbClientMgr.mainWindow().openModal('dialogs/republish.aspx?rnd=' + this._utils.generateRandom(), 'Republishing entire site', 210, 450);
        },

        actionAssignDomain: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/assignDomain.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId, uiKeys['actions_assignDomain'], 420, 500);
            }
        },

        actionLiveEdit: function() {
            /// <summary></summary>

            window.open("canvas.aspx?redir=/" + UmbClientMgr.mainTree().getActionNode().nodeId + ".aspx", "liveediting");
        },

        actionNew: function() {
            /// <summary>Show the create new modal overlay</summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                if (UmbClientMgr.mainTree().getActionNode().nodeType == "content") {
                    UmbClientMgr.mainWindow().openModal("create.aspx?nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + "&nodeType=" + UmbClientMgr.mainTree().getActionNode().nodeType + "&nodeName=" + UmbClientMgr.mainTree().getActionNode().nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], 425, 600);
                }
                else if (UmbClientMgr.mainTree().getActionNode().nodeType == "initmember") {
                    UmbClientMgr.mainWindow().openModal("create.aspx?nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + "&nodeType=" + UmbClientMgr.mainTree().getActionNode().nodeType + "&nodeName=" + UmbClientMgr.mainTree().getActionNode().nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], 380, 420);
                }
                else {
                    UmbClientMgr.mainWindow().openModal("create.aspx?nodeId=" + UmbClientMgr.mainTree().getActionNode().nodeId + "&nodeType=" + UmbClientMgr.mainTree().getActionNode().nodeType + "&nodeName=" + UmbClientMgr.mainTree().getActionNode().nodeName + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_create'], 270, 420);
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
                UmbClientMgr.mainWindow().openModal("dialogs/sendToTranslation.aspx?id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_sendToTranslate'], 470, 500);
            }
        },

        actionEmptyTranscan: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/emptyTrashcan.aspx", uiKeys['actions_emptyTrashcan'], 220, 500);
            }
        },

        actionImport: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/importDocumentType.aspx?rnd=" + this._utils.generateRandom(), uiKeys['actions_importDocumentType'], 460, 400);
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

            UmbClientMgr.mainWindow().openModal('dialogs/viewAuditTrail.aspx?nodeId=' + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_auditTrail'], 500, 550);
        },

        actionPackage: function() {
            /// <summary></summary>
        },

        actionDelete: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeType == "content" && UmbClientMgr.mainTree().getActionNode().nodeId == '-1')
                return;

            this._debug("actionDelete");

            if (confirm(uiKeys['defaultdialogs_confirmdelete'] + ' "' + UmbClientMgr.mainTree().getActionNode().nodeName + '"?\n\n')) {
                //raise nodeDeleting event
                jQuery(this).trigger("nodeDeleting", []);
                var _this = this;
                umbraco.presentation.webservices.legacyAjaxCalls.Delete(UmbClientMgr.mainTree().getActionNode().nodeId, "", UmbClientMgr.mainTree().getActionNode().nodeType, function() {
                    _this._debug("actionDelete: Raising event");
                    //raise nodeDeleted event
                    jQuery(_this).trigger("nodeDeleted", []);
                });
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
                UmbClientMgr.mainWindow().openModal("dialogs/moveOrCopy.aspx?app=" + this._currApp + "&mode=cut&id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_move'], 460, 500);
            }
        },

        actionCopy: function() {
            /// <summary></summary>

            if (UmbClientMgr.mainTree().getActionNode().nodeId != '-1' && UmbClientMgr.mainTree().getActionNode().nodeType != '') {
                UmbClientMgr.mainWindow().openModal("dialogs/moveOrCopy.aspx?app=" + this._currApp + "&mode=copy&id=" + UmbClientMgr.mainTree().getActionNode().nodeId + '&rnd=' + this._utils.generateRandom(), uiKeys['actions_copy'], 470, 500);
            }
        },
        _debug: function(strMsg) {
            if (this._isDebug) {
                Sys.Debug.trace("AppActions: " + strMsg);
            }
        },
        actionExportCode: function() {
            /// <summary></summary>
            UmbClientMgr.mainWindow().openModal("dialogs/exportCode.aspx", UmbClientMgr.uiKeys()['exportDocumentTypeAsCode'], 250, 400);
            return false;
        }
    }
}