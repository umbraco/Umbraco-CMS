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
    tinymce.create('tinymce.plugins.UmbracoLink', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @method init
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */
        init: function (ed, url) {
            var t = this;

            ed.execCommands.mceAdvLink.func = function () {
                var se = ed.selection;

                // No selection and not in link
                if (se.isCollapsed() && !ed.dom.getParent(se.getNode(), 'A'))
                    return;

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('umbraco_path') + '/plugins/tinymce3/insertLink.aspx',
                    width: 480 + parseInt(ed.getLang('advlink.delta_width', 0)),
                    height: 510 + parseInt(ed.getLang('advlink.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url
                });
            };

        }

    });

    // Register plugin
    tinymce.PluginManager.add('umbracolink', tinymce.plugins.UmbracoLink);
})();
