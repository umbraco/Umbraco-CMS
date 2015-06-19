Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditTemplate = base2.Base.extend({
        //private methods/variables
        _opts: null,

        _openMacroModal: function(alias) {
            
            var self = this;

            UmbClientMgr.openAngularModalWindow({
                template: "views/common/dialogs/insertmacro.html",
                dialogData: {
                    renderingEngine: "WebForms",
                    selectedAlias: alias
                },
                callback: function(data) {
                    UmbEditor.Insert(data.syntax, '', self._opts.editorClientId);
                }
            });
        },

        _insertMacro: function(alias) {
            var macroElement = "umbraco:Macro";
            if (!this._opts.useMasterPages) {
                macroElement = "?UMBRACO_MACRO";
            }
            var cp = macroElement + ' Alias="' + alias + '" runat="server"';
            UmbEditor.Insert('<' + cp + ' />', '', this._opts.editorClientId);
        },

        _insertCodeBlockFromTemplate: function(templateId) {
            var self = this;
            $.ajax({
                type: "POST",
                url: this._opts.umbracoPath + "/webservices/templates.asmx/GetCodeSnippet",
                data: "{templateId: '" + templateId + "'}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function(msg) {

                    var cp = 'umbraco:Macro  runat="server" language="cshtml"';
                    UmbEditor.Insert('\n<' + cp + '>\n' + msg.d, '\n</umbraco:Macro' + '>\n', self._opts.editorClientId);

                }
            });
        },

        _insertCodeBlock: function() {
            var snip = this._umbracoInsertSnippet();
            UmbEditor.Insert(snip.BeginTag, snip.EndTag, this._opts.editorClientId);
        },

        _umbracoInsertSnippet: function() {
            var snip = new UmbracoCodeSnippet();
            var cp = 'umbraco:Macro runat="server" language="cshtml"';
            snip.BeginTag = '\n<' + cp + '>\n';
            snip.EndTag = '\n<' + '/umbraco:Macro' + '>\n';
            snip.TargetId = this._opts.editorClientId;
            return snip;
        },

        // Constructor
        constructor: function(opts) {
            // Merge options with default
            this._opts = $.extend({
                

                // Default options go here
            }, opts);
        },

        init: function() {
            //<summary>Sets up the UI and binds events</summary>

            var self = this;

            //bind to the save event
            this._opts.saveButton.click(function (event) {
                event.preventDefault();
                self.doSubmit();
            });
            
            $("#sb").click(function() {
                self._insertCodeBlock();
            });
            $("#sbMacro").click(function() {
                self._openMacroModal();
            });
            //macro split button
            $('#sbMacro').splitbutton({ menu: '#macroMenu' });
            $("#splitButtonMacro").appendTo("#splitButtonMacroPlaceHolder");

            ////razor macro split button
            $('#sb').splitbutton({ menu: '#codeTemplateMenu' });
            $("#splitButton").appendTo("#splitButtonPlaceHolder");

            $(".macro").click(function() {
                var alias = $(this).attr("rel");
                if ($(this).attr("params") == "1") {
                    self._openMacroModal(alias);
                }
                else {
                    self._insertMacro(alias);
                }
            });

            $(".codeTemplate").click(function() {
                self._insertCodeBlockFromTemplate($(this).attr("rel"));
            });
        },

        doSubmit: function() {            
            this.save(jQuery('#' + this._opts.templateNameClientId).val(), jQuery('#' + this._opts.templateAliasClientId).val(), UmbEditor.GetCode());
        },

        save: function(templateName, templateAlias, codeVal) {
            var self = this;

            $.post(self._opts.restServiceLocation + "SaveTemplate",
                    JSON.stringify({
                        templateName: templateName,
                        templateAlias: templateAlias,
                        templateContents: codeVal,
                        templateId: self._opts.templateId,
                        masterTemplateId: this._opts.masterPageDropDown.val()
                    }),
                    function (e) {
                        if (e.success) {
                            self.submitSuccess(e);
                        } else {
                            self.submitFailure(e.message, e.header);
                        }
                    });
            
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
            }
            
            top.UmbSpeechBubble.ShowMessage('save', header, msg);
            UmbClientMgr.mainTree().setActiveTreeType('templates');
            if (pathChanged) {
                UmbClientMgr.mainTree().moveNode(this._opts.templateId, path);
            }
            else {
                UmbClientMgr.mainTree().syncTree(path, true);
            }

        },

        submitFailure: function (err, header) {
            top.UmbSpeechBubble.ShowMessage('error', header, err);
        }
    });
    
    //Set defaults for jQuery ajax calls.
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8'
    });

})(jQuery);