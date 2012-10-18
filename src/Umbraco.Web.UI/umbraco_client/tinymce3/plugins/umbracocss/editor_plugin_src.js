/**
* $Id: editor_plugin_src.js 201 2007-02-12 15:56:56Z spocke $
*
* @author Moxiecode
* @copyright Copyright © 2004-2008, Moxiecode Systems AB, All rights reserved.
*/

(function () {
    // Load plugin specific language pack
    //    tinymce.PluginManager.requireLangPack('umbraco');

    tinymce.create('tinymce.plugins.umbracocss', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */
        init: function (ed, url) {

            this.editor = ed;

            // Register the command so that it can be invoked by using tinyMCE.activeEditor.execCommand('mceExample');
            ed.addCommand('mceumbracosetstyle', function () {
                alert('blah');
            });


            // Add a node change handler, selects the button in the UI when a image is selected
            ed.onNodeChange.add(function (ed, cm, n) {
                var c = cm.get('umbracostyles');
                var formatSelected = false;

                if (c) {
                    // check for element
                    var el = tinymce.DOM.getParent(n, ed.dom.isBlock);
                    if (el) {
                        for (var i = 0; i < c.items.length; i++) {
                            if (c.items[i].value == el.nodeName.toLowerCase()) {
                                c.select(el.nodeName.toLowerCase());
                                formatSelected = true;
                            }
                        }
                    }

                    // check for class
                    if (n.className != '') {
                        if (c) {
                            c.select('.' + n.className);
                        }
                    } else if (c && !formatSelected) {
                        c.select(); // reset selector if no class or block elements
                    }
                }

                /*                if (c = cm.get('styleselect')) {
                if (n.className) {
                t._importClasses();
                c.select(n.className);
                } else
                c.select();
                }

                if (c = cm.get('formatselect')) {
                p = DOM.getParent(n, DOM.isBlock);

                if (p)
                c.select(p.nodeName.toLowerCase());
                }
                */
            });
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
        createControl: function (n, cm) {

            // add style dropdown
            if (n == 'umbracocss') {

                var umbracoStyles = this.editor.getParam('theme_umbraco_styles').split(';');

                var styles = cm.createListBox('umbracostyles', {
                    title: this.editor.getLang('umbraco.style_select'),
                    onselect: function (v) {
                        if (v == '') {
                            if (styles.selectedValue.indexOf('.') == 0) {
                                // remove style
                                var selectedStyle = styles.selectedValue;
                                var styleObj = tinymce.activeEditor.formatter.get('umb' + selectedStyle.substring(1, selectedStyle.length));
                                if (styleObj == undefined) {
                                    tinymce.activeEditor.formatter.register('umb' + selectedStyle.substring(1, selectedStyle.length), {
                                        inline: 'span',
                                        selector: '*',
                                        classes: selectedStyle.substring(1, selectedStyle.length)
                                    });
                                }
                                tinyMCE.activeEditor.formatter.remove('umb' + selectedStyle.substring(1, selectedStyle.length));

                                //                                tinymce.activeEditor.execCommand('mceSetStyleInfo', 0, { command: 'removeformat' });
                            } else {
                                // remove block element
                                tinymce.activeEditor.execCommand('FormatBlock', false, 'p');
                            }
                        }
                        else if (v.indexOf('.') != '0') {
                            tinymce.activeEditor.execCommand('FormatBlock', false, v);
                        } else {
                            // use new formatting engine
                            if (tinymce.activeEditor.formatter.get('umb' + v.substring(1, v.length)) == undefined) {
                                tinymce.activeEditor.formatter.register('umb' + v.substring(1, v.length), {
                                    inline: 'span',
                                    selector: '*',
                                    classes: v.substring(1, v.length)
                                });
                            }
                            var styleObj = tinymce.activeEditor.formatter.get('umb' + v.substring(1, v.length));
                            tinyMCE.activeEditor.formatter.apply('umb' + v.substring(1, v.length));

                            //                            tinyMCE.activeEditor.execCommand('mceSetCSSClass', false, v.substring(1, v.length));

                        }
                        return false;
                    }
                });

                // add styles
                for (var i = 0; i < umbracoStyles.length; i++) {
                    if (umbracoStyles[i] != '') {
                        var name = umbracoStyles[i].substring(0, umbracoStyles[i].indexOf("="));
                        var alias = umbracoStyles[i].substring(umbracoStyles[i].indexOf("=") + 1, umbracoStyles[i].length);

                        if (alias.indexOf('.') < 0)
                            alias = alias.toLowerCase();
                        else if (alias.length > 1) {
                            // register with new formatter engine (can't access from here so a hack in the set style above!)
                            //                            tinyMCE.activeEditor.formatter.register('umb' + alias.substring(1, alias.length), {
                            //                                classes: alias.substring(1, alias.length)
                            //                            });
                        }
                        styles.add(name, alias);
                    }
                }


                return styles;
            }

            return null;
        },


        /**
        * Returns information about the plugin as a name/value array.
        * The current keys are longname, author, authorurl, infourl and version.
        *
        * @return {Object} Name/value array containing information about the plugin.
        */
        getInfo: function () {
            return {
                longname: 'Umbraco CSS/Styling Plugin',
                author: 'Umbraco',
                authorurl: 'http://umbraco.org',
                infourl: 'http://wiki.moxiecode.com/index.php/TinyMCE:Plugins/example',
                version: "1.0"
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('umbracocss', tinymce.plugins.umbracocss);
})();