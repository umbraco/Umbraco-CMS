Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {

    Umbraco.Editors.EditScript = base2.Base.extend({
            //private methods/variables
            _opts: null,

            // Constructor
            constructor: function(opts) {
                // Merge options with default
                this._opts = $.extend({
                    
                    // Default options go here
                }, opts);
            },

            save: function() {
                var self = this;

                umbraco.presentation.webservices.codeEditorSave.SaveCss(
                    self._opts.fileName, self._opts.oldName, self._opts.codeVal, self._opts.cssId,
                    function(t) { self.submitSucces(t); },
                    function(t) { self.submitFailure(t); });

            },

            submitSucces: function(t) {
                if (t != 'true') {
                    top.UmbSpeechBubble.ShowMessage('error', this._opts.text.cssErrorHeader, this._opts.text.cssErrorText);
                }
                else {
                    top.UmbSpeechBubble.ShowMessage('save', this._opts.text.cssSavedHeader, this._opts.text.cssSavedText);
                }
                
                UmbClientMgr.mainTree().setActiveTreeType('stylesheets');
                UmbClientMgr.mainTree().syncTree("-1,init," + this._opts.cssId, true);
            },

            submitFailure: function(t) {
                top.UmbSpeechBubble.ShowMessage('error', this._opts.text.cssErrorHeader, this._opts.text.cssErrorText);
            }
        },
        {
            saveScript: function(codeVal, fileName, oldName, cssId) {
                //<summary>Static method to do the saving</summary>

                var codeVal = $('#' + editorSourceClientId).val();
                //if CodeMirror is not defined, then the code editor is disabled.
                if (typeof(CodeMirror) != "undefined") {
                    codeVal = UmbEditor.GetCode();
                }

                var processor = new Umbraco.Editors.EditStyleSheet({
                    codeVal: codeVal,
                    fileName: $('#' + nameTxtClientId).val(),
                    oldname: nameTxtValue,
                    cssId: cssId
                });
                processor.save();
            }
        });
})(jQuery);