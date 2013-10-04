/**
* $Id: editor_plugin_src.js 201 2007-02-12 15:56:56Z spocke $
*
* @author Moxiecode
* @copyright Copyright © 2004-2008, Moxiecode Systems AB, All rights reserved.
*/

(function() {
    // Load plugin specific language pack
//    tinymce.PluginManager.requireLangPack('umbraco');

    tinymce.create('tinymce.plugins.umbracomacro', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */
        init: function(ed, url) {
            var t = this;

            // Register the command so that it can be invoked by using tinyMCE.activeEditor.execCommand('mceExample');
            ed.addCommand('mceumbracomacro', function() {
                var se = ed.selection;

                var urlParams = "";
                var el = se.getNode();

                // ie selector bug
                if (!ed.dom.hasClass(el, 'umbMacroHolder')) {
                    el = ed.dom.getParent(el, 'div.umbMacroHolder');
                }

                var attrString = "";
                if (ed.dom.hasClass(el, 'umbMacroHolder')) {
                    for (var i = 0; i < el.attributes.length; i++) {
                        attrName = el.attributes[i].nodeName.toLowerCase();
                        if (attrName != "mce_serialized") {
                            if (el.attributes[i].nodeValue && (attrName != 'ismacro' && attrName != 'style' && attrName != 'contenteditable')) {
                                attrString += el.attributes[i].nodeName + '=' + escape(t._utf8_encode(el.attributes[i].nodeValue)) + '&'; //.replace(/#/g, "%23").replace(/\</g, "%3C").replace(/\>/g, "%3E").replace(/\"/g, "%22") + '&';

                            }
                        }
                    }

                    // vi trunkerer strengen ved at fjerne et evt. overskydende amp;
                    if (attrString.length > 0)
                        attrString = attrString.substr(0, attrString.length - 1);

                    urlParams = "&" + attrString;
                } else {
                    urlParams = '&umbPageId=' + tinyMCE.activeEditor.getParam('theme_umbraco_pageId') + '&umbVersionId=' + tinyMCE.activeEditor.getParam('theme_umbraco_versionId');
                }

                ed.windowManager.open({
                    file: tinyMCE.activeEditor.getParam('umbraco_path') + '/plugins/tinymce3/insertMacro.aspx?editor=trueurl' + urlParams,
                    width: 480 + parseInt(ed.getLang('umbracomacro.delta_width', 0)),
                    height: 470 + parseInt(ed.getLang('umbracomacro.delta_height', 0)),
                    inline: 1
                }, {
                    plugin_url: url // Plugin absolute URL
                });
            });

            // Register example button
            ed.addButton('umbracomacro', {
                title: 'umbracomacro.desc',
                cmd: 'mceumbracomacro',
                image: url + '/img/insMacro.gif'
            });

            // Add a node change handler, test if we're editing a macro
            ed.onNodeChange.addToTop(function(ed, cm, n) {

                var macroElement = ed.dom.getParent(ed.selection.getStart(), 'div.umbMacroHolder');

                // mark button if it's a macro
                cm.setActive('umbracomacro', macroElement && ed.dom.hasClass(macroElement, 'umbMacroHolder'));

            });
        },

        _utf8_encode: function(string) {
            string = string.replace(/\r\n/g, "\n");
            var utftext = "";

            for (var n = 0; n < string.length; n++) {

                var c = string.charCodeAt(n);

                if (c < 128) {
                    utftext += String.fromCharCode(c);
                }
                else if ((c > 127) && (c < 2048)) {
                    utftext += String.fromCharCode((c >> 6) | 192);
                    utftext += String.fromCharCode((c & 63) | 128);
                }
                else {
                    utftext += String.fromCharCode((c >> 12) | 224);
                    utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                    utftext += String.fromCharCode((c & 63) | 128);
                }

            }

            return utftext;
        },

        /**
        * Creates control instances based in the incomming name. This method is normally not
        * needed since the addButton method of the tinymce.Editor class is a more easy way of adding buttons
        * but you sometimes need to create more complex controls like listboxes, split buttons etc then this
        * method can be used to create those.
        *
        * @param {String} n Name of the control to create.
        * @param {tinymce.ControlManager} cm Control manager to use inorder to create new control.
        * @return {tinymce.ui.Control} New control instance or null if no control was created.
        */
        createControl: function(n, cm) {
            return null;
        },


        /**
        * Returns information about the plugin as a name/value array.
        * The current keys are longname, author, authorurl, infourl and version.
        *
        * @return {Object} Name/value array containing information about the plugin.
        */
        getInfo: function() {
            return {
                longname: 'Umbraco Macro Insertion Plugin',
                author: 'Umbraco',
                authorurl: 'http://umbraco.org',
                infourl: 'http://umbraco.org/redir/tinymcePlugins',
                version: "1.0"
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('umbracomacro', tinymce.plugins.umbracomacro);
})();