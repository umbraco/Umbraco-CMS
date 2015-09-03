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

            var filename = this._opts.nameTxtBox.val();
            var codeval = this._opts.editorSourceElement.val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof (CodeMirror) != "undefined") {
                codeval = UmbEditor.GetCode();
            }

            this.save(
                    filename,
                    self._opts.originalFileName,
                    codeval);
        },

        save: function (filename, oldName, contents) {
            var self = this;

            $.post(self._opts.restServiceLocation + "SaveScript",
                    JSON.stringify({
                        filename: filename,
                        oldName: oldName,
                        contents: contents
                    }),
                    function (e) {
                        if (e.success) {
                            self.submitSuccess(e);
                        } else {
                            self.submitFailure(e.message, e.header);
                        }
                    });
        },

        submitSuccess: function(args) {
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

            top.UmbSpeechBubble.ShowMessage("save", header, msg);
            UmbClientMgr.mainTree().setActiveTreeType("scripts");
            if (pathChanged) {
                UmbClientMgr.mainTree().moveNode(this._opts.originalFileName, path);
                this._opts.treeSyncPath = args.path;
                this._opts.lttPathElement.prop("href", args.url).html(args.url);
            }
            else {
                UmbClientMgr.mainTree().syncTree(path, true);
            }

            this._opts.lttPathElement.prop("href", args.url).html(args.url);
            this._opts.originalFileName = args.name;
        },

        submitFailure: function(err, header) {
            top.UmbSpeechBubble.ShowMessage('error', header, err);
        }
    });
})(jQuery);