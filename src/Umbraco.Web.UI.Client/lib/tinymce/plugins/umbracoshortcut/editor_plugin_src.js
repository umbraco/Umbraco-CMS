
(function () {
    tinymce.create('tinymce.plugins.Umbracoshortcut', {
        init: function (ed, url) {
            var t = this;
            var ctrlPressed = false;

            t.editor = ed;

            ed.onKeyDown.add(function (ed, e) {
                if (e.keyCode == 17)
                    ctrlPressed = true;

                if (ctrlPressed && e.keyCode == 83) {
                    jQuery(document).trigger("UMBRACO_TINYMCE_SAVE", e);
                    ctrlPressed = false;
                    tinymce.dom.Event.cancel(e);
                    return false;
                }
            });

            ed.onKeyUp.add(function (ed, e) {
                if (e.keyCode == 17)
                    ctrlPressed = false;
            });
        },

        getInfo: function () {
            return {
                longname: 'Umbraco Save short cut key',
                author: 'Umbraco HQ',
                authorurl: 'http://umbraco.com',
                infourl: 'http://our.umbraco.org',
                version: "1.0"
            };
        }

        // Private methods
    });

    // Register plugin
    tinymce.PluginManager.add('umbracoshortcut', tinymce.plugins.Umbracoshortcut);
})();