Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditView = base2.Base.extend({
        /// <summary>Defines the EditView class to controll the persisting of the view file and UI interaction</summary>

        //private methods/variables
        _opts: null,

        _updateNewProperties: function(filePath) {
            /// <summary>Updates the current treeSyncPath and original file name to have the new file name</summary>
            
            //update the originalFileName prop
            this._opts.originalFileName = filePath;

            //re-create the new path
            var subPath = this._opts.treeSyncPath.split(",");
            //remove the last element
            subPath.pop();
            //add the new element
            subPath.push(filePath.split("/")[1]);
            this._opts.treeSyncPath = subPath.join();
        },

        // Constructor
        constructor: function (opts) {
            // Merge options with default
            this._opts = $.extend({
                // Default options go here
            }, opts);
        },

        //public methods/variables

        init: function () {
            var self = this;
            //bind to the change of the master template drop down
            this._opts.masterPageDropDown.change(function () {
                self.changeMasterPageFile();
            });
            //bind to the save event
            this._opts.saveButton.click(function() {
                self.doSubmit();
            });
        },

        insertMacroMarkup: function(alias) {
            /// <summary>callback used to insert the markup for a macro with no parameters</summary>
            
            UmbEditor.Insert("@Umbraco.RenderMacro(\"" + alias + "\")", "", this._opts.codeEditorElementId);
        },
        
        openMacroModal: function (alias) {
            /// <summary>callback used to display the modal dialog to insert a macro with parameters</summary>
            var t = "";
            if (alias != null && alias != "") {
                t = "&alias=" + alias;
            }
            UmbClientMgr.openModalWindow(
                this._opts.modalUrl + '?renderingEngine=Mvc&objectId=' + this._opts.codeEditorElementId + t,
                'Insert Macro', true, 470, 530, 0, 0, '', '');
        },

        doSubmit: function () {
            /// <summary>Submits the data to the server for saving</summary>
            var codeVal = UmbClientMgr.contentFrame().UmbEditor.GetCode();
            var self = this;

            if (this._opts.editorType == "Template") {
                //saving a template view

                $.post(self._opts.restServiceLocation + "SaveTemplate",
                    JSON.stringify({
                        templateName: this._opts.nameTxtBox.val(),
                        templateAlias: this._opts.aliasTxtBox.val(),
                        templateContents: codeVal,
                        templateId: this._opts.templateId,
                        masterTemplateId: this._opts.masterPageDropDown.val()
                    }),
                    function(e) {
                        if (e.success) {
                            self.submitSuccess(e);
                        } else {
                            self.submitFailure(e.message, e.header);
                        }
                    });
            }
            else {
                //saving a partial view    

                $.post(self._opts.restServiceLocation + "SavePartialView",
                    JSON.stringify({
                        filename: this._opts.nameTxtBox.val(),
                        oldName: this._opts.originalFileName,
                        contents: codeVal
                    }),
                    function(e) {
                        if (e.success) {
                            self.submitSuccess(e);
                        } else {
                            self.submitFailure(e.message, e.header);
                        }
                    });
            }
        },
        
        submitSuccess: function (args) {
            
            var msg = args.message;
            var header = args.header;
            var path = this._opts.treeSyncPath;
            var pathChanged = false;
            if (args.path) {
                if (path != args.path) {
                    pathChanged = true;
                }
                path = args.path;
            }

            top.UmbSpeechBubble.ShowMessage('save', header, msg);
            
            UmbClientMgr.mainTree().setActiveTreeType(this._opts.currentTreeType);

            var newFilePath = this._opts.nameTxtBox.val();

            if (this._opts.editorType == "Template") {
                //templates are different because they are ID based, whereas view files are file based without a static id

                if (pathChanged) {
                    UmbClientMgr.mainTree().moveNode(this._opts.templateId, path);
                }
                else {
                    UmbClientMgr.mainTree().syncTree(path, true);
                }

                
            }
            else {
                //we need to pass in the newId parameter so it knows which node to resync after retreival from the server
                UmbClientMgr.mainTree().syncTree(path, true, null, newFilePath.split("/")[1]);
            }

            //then we need to update our current tree sync path to represent the new one
            this._updateNewProperties(newFilePath);
        },
        
        submitFailure: function (err, header) {
            top.UmbSpeechBubble.ShowMessage('error', header, err);                       
        },
        
        changeMasterPageFile: function ( ) {
            //var editor = document.getElementById(this._opts.sourceEditorId);
            var templateDropDown = this._opts.masterPageDropDown.get(0);
            var templateCode = UmbClientMgr.contentFrame().UmbEditor.GetCode();
            var newValue = templateDropDown.options[templateDropDown.selectedIndex].id;

            var layoutDefRegex = new RegExp("(@{[\\s\\S]*?Layout\\s*?=\\s*?)(\"[^\"]*?\"|null)(;[\\s\\S]*?})", "gi");

            if (newValue != undefined && newValue != "") {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1\"" + newValue + "\"$3");
                } else {
                    // Declaration doesn't exist, so prepend to start of doc
                    //TODO: Maybe insert at the cursor position, rather than just at the top of the doc?
                    templateCode = "@{\n\tLayout = \"" + newValue + "\";\n}\n" + templateCode;
                }
            } else {
                if (layoutDefRegex.test(templateCode)) {
                    // Declaration exists, so just update it
                    templateCode = templateCode.replace(layoutDefRegex, "$1null$3");
                }
            }

            UmbClientMgr.contentFrame().UmbEditor.SetCode(templateCode);

            return false;
        }
    });


    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'        
    });

})(jQuery);