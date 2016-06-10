Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditView = base2.Base.extend({
        /// <summary>Defines the EditView class to controll the persisting of the view file and UI interaction</summary>

        //private methods/variables
        _opts: null,

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
            this._opts.saveButton.click(function (event) {
                event.preventDefault();
                self.doSubmit();
            });
        },

        insertMacroMarkup: function(alias) {
            /// <summary>callback used to insert the markup for a macro with no parameters</summary>

            UmbEditor.Insert("@Umbraco.RenderMacro(\"" + alias + "\")", "", this._opts.codeEditorElementId);
        },

        insertRenderBody: function() {
            UmbEditor.Insert("@RenderBody()", "", this._opts.codeEditorElementId);
        },

        openMacroModal: function (alias) {
            /// <summary>callback used to display the modal dialog to insert a macro with parameters</summary>

            var self = this;

            UmbClientMgr.openAngularModalWindow({
                template: "views/common/dialogs/insertmacro.html",
                dialogData: {
                    renderingEngine: "Mvc",
                    macroData: {macroAlias: alias}
                },
                callback: function (data) {
                    UmbEditor.Insert(data.syntax, '', self._opts.codeEditorElementId);
                }
            });
        },

        openSnippetModal: function (type) {
            /// <summary>callback used to display the modal dialog to insert a macro with parameters</summary>

            var self = this;

            UmbClientMgr.openAngularModalWindow({
                template: "views/common/dialogs/template/snippet.html",
                callback: function (data) {

                    var code = "";

                    if (type === 'section') {
                        code = "\n@section " + data.name + "{\n";
                        code += "<!-- Content here -->\n" +
                            "}\n";
                    }

                    if (type === 'rendersection') {
                        if (data.required) {
                            code = "\n@RenderSection(\"" + data.name + "\", true)\n";
                        } else {
                            code = "\n@RenderSection(\"" + data.name + "\", false)\n";
                        }
                    }

                    UmbEditor.Insert(code, '', self._opts.codeEditorElementId);
                },
                type: type
            });
        },


        openQueryModal: function () {
            /// <summary>callback used to display the modal dialog to insert a macro with parameters</summary>

            var self = this;

            UmbClientMgr.openAngularModalWindow({
                template: "views/common/dialogs/template/queryBuilder.html",
                callback: function (data) {

                    //var dataFormatted = data.replace(new RegExp('[' + "." + ']', 'g'), "\n\t\t\t\t\t.");

                    var code = "\n@{\n" + "\tvar selection = " + data + ";\n}\n";
                    code += "<ul>\n" +
                                "\t@foreach(var item in selection){\n" +
                                    "\t\t<li>\n" +
                                        "\t\t\t<a href=\"@item.Url\">@item.Name</a>\n" +
                                    "\t\t</li>\n" +
                                "\t}\n" +
                            "</ul>\n\n";

                    UmbEditor.Insert(code, '', self._opts.codeEditorElementId);
                }
            });
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
                var actionName = this._opts.editorType === "PartialViewMacro" ? "SavePartialViewMacro" : "SavePartialView";

                $.post(self._opts.restServiceLocation + actionName,
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
            if (args.contents) {
                UmbEditor.SetCode(args.contents);
            } else if (!this.IsSimpleEditor) {
                // Restore focuse to text region. SetCode also does this.
                UmbEditor._editor.focus();
            }

            UmbClientMgr.mainTree().setActiveTreeType(this._opts.currentTreeType);

            if (this._opts.editorType == "Template") {

                var alias = args.alias;
                this._opts.aliasTxtBox.val(alias);

                top.UmbSpeechBubble.ShowMessage('save', header, msg);

                //templates are different because they are ID based, whereas view files are file based without a static id

                if (pathChanged) {
                    UmbClientMgr.mainTree().moveNode(this._opts.templateId, path);
                    this._opts.treeSyncPath = path;
                }
                else {
                    UmbClientMgr.mainTree().syncTree(path, true);
                }

            }
            else {
                var newFilePath = this._opts.nameTxtBox.val();


                function trimStart(str, trim) {
                    if (str.startsWith(trim)) {
                        return str.substring(trim.length);
                    }
                    return str;
                }

                //if the filename changes, we need to redirect since the file name is used in the url
                if (this._opts.originalFileName != newFilePath) {
                    var queryParts = trimStart(window.location.search, "?").split('&');
                    var notFileParts = [];
                    for (var i = 0; i < queryParts.length; i++) {
                        if (queryParts[i].substr(0, "file=".length) != "file=") {
                            notFileParts.push(queryParts[i]);
                        }
                    }
                    var newLocation = window.location.pathname + "?" + notFileParts.join("&") + "&file=" + newFilePath;

                    UmbClientMgr.contentFrame(newLocation);

                    //we need to do this after we navigate otherwise the navigation will wait unti lthe message timeout is done!
                    top.UmbSpeechBubble.ShowMessage('save', header, msg);
                }
                else {

                    top.UmbSpeechBubble.ShowMessage('save', header, msg);

                    if (args && args.name) {
                        this._opts.originalFileName = args.name;
                    }
                    if (args && args.path) {
                        this._opts.treeSyncPath = args.path;
                    }

                    UmbClientMgr.mainTree().syncTree(path, true, null, newFilePath.split("/")[1]);
                }
            }

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