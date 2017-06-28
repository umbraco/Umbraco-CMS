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
    * This plugin modifies the standard TinyMCE context menu, with umbraco specific changes.
    *
    * @class tinymce.plugins.umbContextMenu
    */
    tinymce.create('tinymce.plugins.UmbracoContextMenu', {
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
            if (ed.plugins.contextmenu) {

            ed.plugins.contextmenu.onContextMenu.add(function (th, menu, event) {

                var keys = UmbClientMgr.uiKeys();

                $.each(menu.items, function (idx, el) {
                
                    switch (el.settings.cmd) {
                        case "Cut":
                            el.settings.title = keys['defaultdialogs_cut'];
                            break;
                        case "Copy":
                            el.settings.title = keys['general_copy'];
                            break;
                        case "Paste":
                            el.settings.title = keys['defaultdialogs_paste'];
                            break;
                        case "mceAdvLink":
                        case "mceLink":
                            el.settings.title = keys['defaultdialogs_insertlink'];
                            break;
                        case "UnLink":
                            el.settings.title = keys['relatedlinks_removeLink'];
                            break;
                        case "mceImage":
                            el.settings.title = keys['defaultdialogs_insertimage'];
                            el.settings.cmd = "mceUmbimage";
                            break;
                    }

                });

            });
        }
        }
    });

    // Register plugin
    tinymce.PluginManager.add('umbracocontextmenu', tinymce.plugins.UmbracoContextMenu);
})();
