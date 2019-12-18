angular.module('umbraco.mocks').
    factory('prevaluesMocks', ['$httpBackend', 'mocksUtils', function ($httpBackend, mocksUtils) {
        'use strict';

        function getRichTextConfiguration(status, data, headers) {
            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }
            else {
                return [200, { "plugins": [{ "name": "code", "useOnFrontend": true }, { "name": "paste", "useOnFrontend": true }, { "name": "umbracolink", "useOnFrontend": true }], "commands": [{ "icon": "images/editor/code.gif", "command": "code", "alias": "code", "frontEndCommand": "code", "value": "", "priority": 1, "isStylePicker": false }, { "icon": "images/editor/removeformat.gif", "command": "removeformat", "alias": "removeformat", "frontEndCommand": "removeformat", "value": "", "priority": 2, "isStylePicker": false }, { "icon": "images/editor/undo.gif", "command": "undo", "alias": "undo", "frontEndCommand": "undo", "value": "", "priority": 11, "isStylePicker": false }, { "icon": "images/editor/redo.gif", "command": "redo", "alias": "redo", "frontEndCommand": "redo", "value": "", "priority": 12, "isStylePicker": false }, { "icon": "images/editor/cut.gif", "command": "cut", "alias": "cut", "frontEndCommand": "cut", "value": "", "priority": 13, "isStylePicker": false }, { "icon": "images/editor/copy.gif", "command": "copy", "alias": "copy", "frontEndCommand": "copy", "value": "", "priority": 14, "isStylePicker": false }, { "icon": "images/editor/showStyles.png", "command": "styleselect", "alias": "styleselect", "frontEndCommand": "styleselect", "value": "", "priority": 20, "isStylePicker": false }, { "icon": "images/editor/bold.gif", "command": "bold", "alias": "bold", "frontEndCommand": "bold", "value": "", "priority": 21, "isStylePicker": false }, { "icon": "images/editor/italic.gif", "command": "italic", "alias": "italic", "frontEndCommand": "italic", "value": "", "priority": 22, "isStylePicker": false }, { "icon": "images/editor/underline.gif", "command": "underline", "alias": "underline", "frontEndCommand": "underline", "value": "", "priority": 23, "isStylePicker": false }, { "icon": "images/editor/strikethrough.gif", "command": "strikethrough", "alias": "strikethrough", "frontEndCommand": "strikethrough", "value": "", "priority": 24, "isStylePicker": false }, { "icon": "images/editor/justifyleft.gif", "command": "justifyleft", "alias": "justifyleft", "frontEndCommand": "alignleft", "value": "", "priority": 31, "isStylePicker": false }, { "icon": "images/editor/justifycenter.gif", "command": "justifycenter", "alias": "justifycenter", "frontEndCommand": "aligncenter", "value": "", "priority": 32, "isStylePicker": false }, { "icon": "images/editor/justifyright.gif", "command": "justifyright", "alias": "justifyright", "frontEndCommand": "alignright", "value": "", "priority": 33, "isStylePicker": false }, { "icon": "images/editor/justifyfull.gif", "command": "justifyfull", "alias": "justifyfull", "frontEndCommand": "alignfull", "value": "", "priority": 34, "isStylePicker": false }, { "icon": "images/editor/bullist.gif", "command": "bullist", "alias": "bullist", "frontEndCommand": "bullist", "value": "", "priority": 41, "isStylePicker": false }, { "icon": "images/editor/numlist.gif", "command": "numlist", "alias": "numlist", "frontEndCommand": "numlist", "value": "", "priority": 42, "isStylePicker": false }, { "icon": "images/editor/outdent.gif", "command": "outdent", "alias": "outdent", "frontEndCommand": "outdent", "value": "", "priority": 43, "isStylePicker": false }, { "icon": "images/editor/indent.gif", "command": "indent", "alias": "indent", "frontEndCommand": "indent", "value": "", "priority": 44, "isStylePicker": false }, { "icon": "images/editor/link.gif", "command": "link", "alias": "mcelink", "frontEndCommand": "link", "value": "", "priority": 51, "isStylePicker": false }, { "icon": "images/editor/unLink.gif", "command": "unlink", "alias": "unlink", "frontEndCommand": "unlink", "value": "", "priority": 52, "isStylePicker": false }, { "icon": "images/editor/anchor.gif", "command": "anchor", "alias": "mceinsertanchor", "frontEndCommand": "anchor", "value": "", "priority": 53, "isStylePicker": false }, { "icon": "images/editor/image.gif", "command": "image", "alias": "mceimage", "frontEndCommand": "umbmediapicker", "value": "", "priority": 61, "isStylePicker": false }, { "icon": "images/editor/insMacro.gif", "command": "umbracomacro", "alias": "umbracomacro", "frontEndCommand": "umbmacro", "value": "", "priority": 62, "isStylePicker": false }, { "icon": "images/editor/table.gif", "command": "table", "alias": "mceinserttable", "frontEndCommand": "table", "value": "", "priority": 63, "isStylePicker": false }, { "icon": "images/editor/media.gif", "command": "umbracoembed", "alias": "umbracoembed", "frontEndCommand": "umbembeddialog", "value": "", "priority": 66, "isStylePicker": false }, { "icon": "images/editor/hr.gif", "command": "hr", "alias": "inserthorizontalrule", "frontEndCommand": "hr", "value": "", "priority": 71, "isStylePicker": false }, { "icon": "images/editor/sub.gif", "command": "sub", "alias": "subscript", "frontEndCommand": "sub", "value": "", "priority": 72, "isStylePicker": false }, { "icon": "images/editor/sup.gif", "command": "sup", "alias": "superscript", "frontEndCommand": "sup", "value": "", "priority": 73, "isStylePicker": false }, { "icon": "images/editor/charmap.gif", "command": "charmap", "alias": "mcecharmap", "frontEndCommand": "charmap", "value": "", "priority": 74, "isStylePicker": false }], "validElements": "+a[id|style|rel|rev|charset|hreflang|dir|lang|tabindex|accesskey|type|name|href|target|title|class|onfocus|onblur|onclick|ondblclick|onmousedown|onmouseup|onmouseover|onmousemove|onmouseout|onkeypress|onkeydown|onkeyup],-strong/-b[class|style],-em/-i[class|style],-strike[class|style],-u[class|style],#p[id|style|dir|class|align],-ol[class|reversed|start|style|type],-ul[class|style],-li[class|style],br[class],img[id|dir|lang|longdesc|usemap|style|class|src|onmouseover|onmouseout|border|alt=|title|hspace|vspace|width|height|align|umbracoorgwidth|umbracoorgheight|onresize|onresizestart|onresizeend|rel],-sub[style|class],-sup[style|class],-blockquote[dir|style|class],-table[border=0|cellspacing|cellpadding|width|height|class|align|summary|style|dir|id|lang|bgcolor|background|bordercolor],-tr[id|lang|dir|class|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor],tbody[id|class],thead[id|class],tfoot[id|class],#td[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|bgcolor|background|bordercolor|scope],-th[id|lang|dir|class|colspan|rowspan|width|height|align|valign|style|scope],caption[id|lang|dir|class|style],-div[id|dir|class|align|style],-span[class|align|style],-pre[class|align|style],address[class|align|style],-h1[id|dir|class|align],-h2[id|dir|class|align],-h3[id|dir|class|align],-h4[id|dir|class|align],-h5[id|dir|class|align],-h6[id|style|dir|class|align],hr[class|style],dd[id|class|title|style|dir|lang],dl[id|class|title|style|dir|lang],dt[id|class|title|style|dir|lang],object[class|id|width|height|codebase|*],param[name|value|_value|class],embed[type|width|height|src|class|*],map[name|class],area[shape|coords|href|alt|target|class],bdo[class],button[class],iframe[*]", "inValidElements": "font", "customConfig": { "entity_encoding": "raw" } }, null];
            }
        }

        function getCanvasEditorConfiguration(status, data, headers) {
            if (!mocksUtils.checkAuth()) {
                return [401, null, null];
            }
            else {
                return [200,
                    [
                        {
                            "name": "Rich text editor",
                            "alias": "rte",
                            "view": "rte",
                            "icon": "icon-article"
                        },
                        {
                            "name": "Image",
                            "alias": "media",
                            "view": "media",
                            "icon": "icon-picture"
                        },
                        {
                            "name": "Macro",
                            "alias": "macro",
                            "view": "macro",
                            "icon": "icon-settings-alt"
                        },
                        {
                            "name": "Embed",
                            "alias": "embed",
                            "view": "embed",
                            "icon": "icon-movie-alt"
                        },
                        {
                            "name": "Headline",
                            "alias": "headline",
                            "view": "textstring",
                            "icon": "icon-coin",
                            "config": {
                                "style": "font-size: 36px; line-height: 45px; font-weight: bold",
                                "markup": "<h1>#value#</h1>"
                            }
                        },
                        {
                            "name": "Quote",
                            "alias": "quote",
                            "view": "textstring",
                            "icon": "icon-quote",
                            "config": {
                                "style": "border-left: 3px solid #ccc; padding: 10px; color: #ccc; font-family: serif; font-style: italic; font-size: 18px",
                                "markup": "<blockquote>#value#</blockquote>"
                            }
                        }
                    ], null];
            }

        }


        return {
            register: function () {
                $httpBackend
                    .whenGET(mocksUtils.urlRegex('/umbraco/UmbracoApi/RichTextPreValue/GetConfiguration'))
                    .respond(getRichTextConfiguration);
                $httpBackend
                    .whenGET(mocksUtils.urlRegex('../config/canvas.editors.config.js'))
                    .respond(getCanvasEditorConfiguration);
            }
        };
    }]);
