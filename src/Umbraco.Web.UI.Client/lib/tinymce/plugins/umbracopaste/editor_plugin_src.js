/**
* editor_plugin_src.js
*
* Copyright 2012, Umbraco
* Released under MIT License.
*
* License: http://opensource.org/licenses/mit-license.html
*/

(function () {
    var Event = tinymce.dom.Event, each = tinymce.each, DOM = tinymce.DOM;

    /**
    * This plugin modifies the standard TinyMCE paste, with umbraco specific changes.
    *
    * @class tinymce.plugins.umbContextMenu
    */
    tinymce.create('tinymce.plugins.UmbracoPaste', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @method init
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */
        init: function (ed) {
            var t = this;

            ed.plugins.paste.onPreProcess.add(function (pl, o) {

                var ed = this.editor, h = o.content;

                var umbracoAllowedStyles = ed.getParam('theme_umbraco_styles');
                for (var i = 1; i < 7; i++) {
                    if (umbracoAllowedStyles.indexOf("h" + i) == -1) {
                        h = h.replace(new RegExp('<h' + i + '[^>]*', 'gi'), '<p><strong');
                        h = h.replace(new RegExp('</h' + i + '>', 'gi'), '</strong></p>');
                    }
                }

                o.content = h;

            });

        }

    });

    // Register plugin
    tinymce.PluginManager.add('umbracopaste', tinymce.plugins.UmbracoPaste);
})();
