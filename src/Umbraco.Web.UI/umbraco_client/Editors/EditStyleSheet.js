Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditStyleSheet = base2.Base.extend({
        //private methods/variables
        _opts: null,

        // Constructor
        constructor: function(opts) {
            // Merge options with default
            this._opts = $.extend({
                

                // Default options go here
            }, opts);
        },

        init: function() {
            //setup UI elements
            var self = this;

            //bind to the save event
            this._opts.saveButton.click(function (event) {
                event.preventDefault();
                self.doSubmit();
            });
        },

        doSubmit: function() {
            var self = this;

            var fileName = this._opts.nameTxtBox.val();
            var codeVal = this._opts.editorSourceElement.val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof(CodeMirror) != "undefined") {
                codeVal = UmbEditor.GetCode();
            }
            umbraco.presentation.webservices.codeEditorSave.SaveCss(
                fileName, self._opts.originalFileName, codeVal, self._opts.cssId,
                function(t) { self.submitSucces(t); },
                function(t) { self.submitFailure(t); });


        },

        submitSucces: function(t) {
            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', unescape(this._opts.text.cssErrorHeader), unescape(this._opts.text.cssErrorText));
            }
            else {
                top.UmbSpeechBubble.ShowMessage('save', unescape(this._opts.text.cssSavedHeader), unescape(this._opts.text.cssSavedText));
            }

            UmbClientMgr.mainTree().setActiveTreeType('stylesheets');
            UmbClientMgr.mainTree().syncTree("-1,init," + this._opts.cssId, true);

            //update the originalFileName prop
            this._opts.originalFileName = this._opts.nameTxtBox.val();
        },

        submitFailure: function(t) {
            top.UmbSpeechBubble.ShowMessage('error', unescape(this._opts.text.cssErrorHeader), unescape(this._opts.text.cssErrorText));
        }
    });
})(jQuery);