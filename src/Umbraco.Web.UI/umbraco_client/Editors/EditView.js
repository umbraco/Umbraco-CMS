Umbraco.Sys.registerNamespace("Umbraco.Editors");

(function ($) {


    Umbraco.Editors.EditView = base2.Base.extend({
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
            this._opts.saveButton.click(function() {
                self.doSubmit();
            });
        },

        doSubmit: function () {
            var codeVal = UmbClientMgr.contentFrame().UmbEditor.GetCode();
            var self = this;

            umbraco.presentation.webservices.codeEditorSave.SaveTemplate(
                this._opts.nameTxtBox.val(),
                this._opts.aliasTxtBox.val(),
                codeVal,
                this._opts.templateId,
                this._opts.masterPageDropDown.val(),
                function (t) { self.submitSuccess(t); },
                function (t) { self.submitFailure(t); });
        },
        
        submitSuccess: function (t) {
            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', this._opts.msgs.templateErrorHeader, this._opts.msgs.templateErrorText);
            }
            else {
                top.UmbSpeechBubble.ShowMessage('save', this._opts.msgs.templateSavedHeader, this._opts.msgs.templateSavedText);
            }
        },
        
        submitFailure: function (t) {
            top.UmbSpeechBubble.ShowMessage('error', this._opts.msgs.templateErrorHeader, this._opts.msgs.templateErrorText);
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



})(jQuery);