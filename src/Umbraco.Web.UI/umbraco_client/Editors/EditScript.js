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

        init: function () {
            //setup UI elements
            var self = this;

            //bind to the save event
            this._opts.saveButton.click(function (event) {
                event.preventDefault();
                self.doSubmit();
            });
        },
        
        doSubmit: function () {
            var self = this;

            var fileName = this._opts.nameTxtBox.val();
            var codeVal = this._opts.editorSourceElement.val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof (CodeMirror) != "undefined") {
                codeVal = UmbEditor.GetCode();
            }
            umbraco.presentation.webservices.codeEditorSave.SaveScript(
                fileName, self._opts.originalFileName, codeVal,
                function (t) { self.submitSucces(t); },
                function (t) { self.submitFailure(t); });

        },

        submitSucces: function(t) {

            if (t != 'true') {
                top.UmbSpeechBubble.ShowMessage('error', unescape(this._opts.text.fileErrorHeader), unescape(this._opts.text.fileErrorText));
            }

            var newFilePath = this._opts.nameTxtBox.val();
            
            //if the filename changes, we need to redirect since the file name is used in the url
            if (this._opts.originalFileName != newFilePath) {                
                var newLocation = window.location.pathname + "?" + "&file=" + newFilePath;

                UmbClientMgr.contentFrame(newLocation);

                //we need to do this after we navigate otherwise the navigation will wait unti lthe message timeout is done!
                top.UmbSpeechBubble.ShowMessage('save', unescape(this._opts.text.fileSavedHeader), unescape(this._opts.text.fileSavedText));
            }
            else {

                top.UmbSpeechBubble.ShowMessage('save', unescape(this._opts.text.fileSavedHeader), unescape(this._opts.text.fileSavedText));
                UmbClientMgr.mainTree().setActiveTreeType('scripts');

                //we need to create a list of ids for each folder/file. Each folder/file's id is it's full path so we need to build each one.
                var paths = [];
                var parts = this._opts.originalFileName.split('/');
                for (var i = 0;i < parts.length;i++) {
                    if (paths.length > 0) {
                        paths.push(paths[i - 1] + "/" + parts[i]);
                    }
                    else {
                        paths.push(parts[i]);
                    }
                }

                //we need to pass in the newId parameter so it knows which node to resync after retreival from the server
                UmbClientMgr.mainTree().syncTree("-1,init," + paths.join(','), true, null, newFilePath);
                //set the original file path to the new one
                this._opts.originalFileName = newFilePath;
            }
            
        },

        submitFailure: function(t) {
            top.UmbSpeechBubble.ShowMessage('error', unescape(this._opts.text.fileErrorHeader), unescape(this._opts.text.fileErrorText));
        }
    });
})(jQuery);