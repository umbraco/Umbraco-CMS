Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditTemplate = base2.Base.extend({
        //private methods/variables
        _opts: null,

        _openMacroModal: function(alias) {
            var t = "";
            if (alias != null && alias != "") {
                t = "&alias=" + alias;
            }
            UmbClientMgr.openModalWindow(this._opts.umbracoPath + '/dialogs/editMacro.aspx?renderingEngine=Webforms&objectId=' + this._opts.editorClientId + t, 'Insert Macro', true, 470, 530, 0, 0, '', '');
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

        save: function(templateName, templateAlias, codeVal) {
            var self = this;

            umbraco.presentation.webservices.codeEditorSave.SaveTemplate(
                templateName, templateAlias, codeVal, self._opts.templateId, this._opts.masterPageDropDown.val(),
                function(t) { self.submitSucces(t); },
                function(t) { self.submitFailure(t); });

        },

        submitSucces: function(t) {
            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', this._opts.text.templateErrorHeader, this._opts.text.templateErrorText);
            }
            else {
                top.UmbSpeechBubble.ShowMessage('save', this._opts.text.templateSavedHeader, this._opts.text.templateSavedText);
            }

            UmbClientMgr.mainTree().setActiveTreeType('templates');
            UmbClientMgr.mainTree().syncTree(this._opts.treeSyncPath, true);
        },

        submitFailure: function(t) {
            top.UmbSpeechBubble.ShowMessage('error', this._opts.text.templateErrorHeader, this._opts.text.templateErrorText);
        }
    });
})(jQuery);