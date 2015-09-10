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

            var filename = this._opts.nameTxtBox.val();
            var codeval = this._opts.editorSourceElement.val();
            //if CodeMirror is not defined, then the code editor is disabled.
            if (typeof(CodeMirror) != "undefined") {
                codeval = UmbEditor.GetCode();
            }

            this.save(
                filename,
                self._opts.originalFileName,
                codeval);
        },

        save: function (filename, oldName, contents) {
            var self = this;

            $.post(self._opts.restServiceLocation + "SaveStylesheet",
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

            UmbClientMgr.mainTree().setActiveTreeType("stylesheets");
            if (pathChanged) {
                // file is used in url so we need to redirect
                var qs = window.location.search;
                if (qs.startsWith("?")) qs = qs.substring("?".length);
                var qp1 = qs.split("&");
                var qp2 = [];
                for (var i = 0; i < qp1.length; i++)
                    if (!qp1[i].startsWith("id="))
                        qp2.push(qp1[i]);

                var location = window.location.pathname + "?" + qp2.join("&") + "&id=" + args.name;
                UmbClientMgr.contentFrame(location);

                // need to do it after we navigate otherwise the navigation waits until the message timeout is done
                top.UmbSpeechBubble.ShowMessage("save", header, msg);
            }
            else {
                top.UmbSpeechBubble.ShowMessage("save", header, msg);
                this._opts.lttPathElement.prop("href", args.url).html(args.url);
                this._opts.originalFileName = args.name;
                this._opts.treeSyncPath = args.path;
                UmbClientMgr.mainTree().syncTree(path, true);
            }
        },

        submitFailure: function(err, header) {
            top.UmbSpeechBubble.ShowMessage('error', header, err);
        }
    });
})(jQuery);