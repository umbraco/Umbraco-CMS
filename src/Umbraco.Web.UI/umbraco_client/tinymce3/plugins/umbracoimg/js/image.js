var ImageDialog = {
    preInit: function() {
        var url;

        tinyMCEPopup.requireLangPack();

        if (url = tinyMCEPopup.getParam("external_image_list_url"))
            document.write('<script language="javascript" type="text/javascript" src="' + tinyMCEPopup.editor.documentBaseURI.toAbsolute(url) + '"></script>');
    },

    init: function(ed) {
        var f = document.forms[0], nl = f.elements, ed = tinyMCEPopup.editor, dom = ed.dom, n = ed.selection.getNode();

        tinyMCEPopup.resizeToInnerSize();

        if (n.nodeName == 'IMG') {
            nl.src.value = dom.getAttrib(n, 'src');
            nl.width.value = dom.getAttrib(n, 'width');
            nl.height.value = dom.getAttrib(n, 'height');
            nl.alt.value = dom.getAttrib(n, 'alt');
            nl.orgHeight.value = dom.getAttrib(n, 'rel').split(",")[1];
            nl.orgWidth.value = dom.getAttrib(n, 'rel').split(",")[0];
            
        }

        // If option enabled default contrain proportions to checked
        if ((ed.getParam("advimage_constrain_proportions", true)) && f.constrain)
            f.constrain.checked = true;

        this.changeAppearance();
        this.showPreviewImage(nl.src.value, 1);
    },

    insert: function(file, title) {
        var ed = tinyMCEPopup.editor, t = this, f = document.forms[0];

        if (f.src.value === '') {
            if (ed.selection.getNode().nodeName == 'IMG') {
                ed.dom.remove(ed.selection.getNode());
                ed.execCommand('mceRepaint');
            }

            tinyMCEPopup.close();
            return;
        }

        if (tinyMCEPopup.getParam("accessibility_warnings", 1)) {
            if (!f.alt.value) {
                tinyMCEPopup.confirm(tinyMCEPopup.getLang('advimage_dlg.missing_alt'), function(s) {
                    if (s)
                        t.insertAndClose();
                });

                return;
            }
        }

        t.insertAndClose();
    },

    insertAndClose: function() {
        var ed = tinyMCEPopup.editor, f = document.forms[0], nl = f.elements, v, args = {}, el;

        tinyMCEPopup.restoreSelection();

        // Fixes crash in Safari
        if (tinymce.isWebKit)
            ed.getWin().focus();

        if (!ed.settings.inline_styles) {
            args = {
                vspace: nl.vspace.value,
                hspace: nl.hspace.value,
                border: nl.border.value,
                align: getSelectValue(f, 'align')
            };
        } else {
            // Remove deprecated values
            args = {
                vspace: '',
                hspace: '',
                border: '',
                align: ''
            };
        }

        tinymce.extend(args, {
            src: nl.src.value,
            width: nl.width.value,
            height: nl.height.value,
            alt: nl.alt.value,
            title: nl.alt.value,
            rel: nl.orgWidth.value + ',' + nl.orgHeight.value
        });

        args.onmouseover = args.onmouseout = '';

        el = ed.selection.getNode();

        if (el && el.nodeName == 'IMG') {
            ed.dom.setAttribs(el, args);
        } else {
            ed.execCommand('mceInsertContent', false, '<img id="__mce_tmp" />', { skip_undo: 1 });
            ed.dom.setAttribs('__mce_tmp', args);
            ed.dom.setAttrib('__mce_tmp', 'id', '');
            ed.undoManager.add();
        }

        tinyMCEPopup.close();
    },

    getAttrib: function(e, at) {
        var ed = tinyMCEPopup.editor, dom = ed.dom, v, v2;

        if (ed.settings.inline_styles) {
            switch (at) {
                case 'align':
                    if (v = dom.getStyle(e, 'float'))
                        return v;

                    if (v = dom.getStyle(e, 'vertical-align'))
                        return v;

                    break;

                case 'hspace':
                    v = dom.getStyle(e, 'margin-left')
                    v2 = dom.getStyle(e, 'margin-right');

                    if (v && v == v2)
                        return parseInt(v.replace(/[^0-9]/g, ''));

                    break;

                case 'vspace':
                    v = dom.getStyle(e, 'margin-top')
                    v2 = dom.getStyle(e, 'margin-bottom');
                    if (v && v == v2)
                        return parseInt(v.replace(/[^0-9]/g, ''));

                    break;

                case 'border':
                    v = 0;

                    tinymce.each(['top', 'right', 'bottom', 'left'], function(sv) {
                        sv = dom.getStyle(e, 'border-' + sv + '-width');

                        // False or not the same as prev
                        if (!sv || (sv != v && v !== 0)) {
                            v = 0;
                            return false;
                        }

                        if (sv)
                            v = sv;
                    });

                    if (v)
                        return parseInt(v.replace(/[^0-9]/g, ''));

                    break;
            }
        }

        if (v = dom.getAttrib(e, at))
            return v;

        return '';
    },

    setSwapImage: function(st) {
        var f = document.forms[0];

        f.onmousemovecheck.checked = st;
        setBrowserDisabled('overbrowser', !st);
        setBrowserDisabled('outbrowser', !st);

        if (f.over_list)
            f.over_list.disabled = !st;

        if (f.out_list)
            f.out_list.disabled = !st;

        f.onmouseoversrc.disabled = !st;
        f.onmouseoutsrc.disabled = !st;
    },

    resetImageData: function() {
        var f = document.forms[0];

        f.elements.width.value = f.elements.height.value = '';
    },

    updateImageData: function(img, st) {
        var f = document.forms[0];

        if (!st) {
            f.elements.width.value = img.width;
            f.elements.height.value = img.height;
        }

        this.preloadImg = img;
    },

    changeAppearance: function() {
        var ed = tinyMCEPopup.editor, f = document.forms[0], img = document.getElementById('alignSampleImg');

        if (img) {
            if (ed.getParam('inline_styles')) {
                ed.dom.setAttrib(img, 'style', f.style.value);
            } else {
                img.align = f.align.value;
                img.border = f.border.value;
                img.hspace = f.hspace.value;
                img.vspace = f.vspace.value;
            }
        }
    },

    changeHeight: function() {
        var f = document.forms[0], tp, t = this;
        alert(t.preloadImg);
        
        if (!f.constrain.checked || !t.preloadImg) {
            return;
        }

        if (f.width.value == '' || f.height.value == '')
            return;

        tp = (parseInt(f.width.value) / parseInt(t.preloadImg.width)) * t.preloadImg.height;
        f.height.value = tp.toFixed(0);
    },

    changeWidth: function() {
        var f = document.forms[0], tp, t = this;

        if (!f.constrain.checked || !t.preloadImg) {
            return;
        }

        if (f.width.value == '' || f.height.value == '')
            return;

        tp = (parseInt(f.height.value) / parseInt(t.preloadImg.height)) * t.preloadImg.width;
        f.width.value = tp.toFixed(0);
    },

    updateStyle: function(ty) {
        var dom = tinyMCEPopup.dom, st, v, f = document.forms[0], img = dom.create('img', { style: dom.get('style').value });

        if (tinyMCEPopup.editor.settings.inline_styles) {
            // Handle align
            if (ty == 'align') {
                dom.setStyle(img, 'float', '');
                dom.setStyle(img, 'vertical-align', '');

                v = getSelectValue(f, 'align');
                if (v) {
                    if (v == 'left' || v == 'right')
                        dom.setStyle(img, 'float', v);
                    else
                        img.style.verticalAlign = v;
                }
            }

            // Handle border
            if (ty == 'border') {
                dom.setStyle(img, 'border', '');

                v = f.border.value;
                if (v || v == '0') {
                    if (v == '0')
                        img.style.border = '0';
                    else
                        img.style.border = v + 'px solid black';
                }
            }

            // Handle hspace
            if (ty == 'hspace') {
                dom.setStyle(img, 'marginLeft', '');
                dom.setStyle(img, 'marginRight', '');

                v = f.hspace.value;
                if (v) {
                    img.style.marginLeft = v + 'px';
                    img.style.marginRight = v + 'px';
                }
            }

            // Handle vspace
            if (ty == 'vspace') {
                dom.setStyle(img, 'marginTop', '');
                dom.setStyle(img, 'marginBottom', '');

                v = f.vspace.value;
                if (v) {
                    img.style.marginTop = v + 'px';
                    img.style.marginBottom = v + 'px';
                }
            }

            // Merge
            dom.get('style').value = dom.serializeStyle(dom.parseStyle(img.style.cssText));
        }
    },

    changeMouseMove: function() {
    },

    showPreviewImage: function(u, st) {
        if (!u) {
            tinyMCEPopup.dom.setHTML('prev', '');
            return;
        }

        if (!st && tinyMCEPopup.getParam("advimage_update_dimensions_onchange", true))
            this.resetImageData();

        u = tinyMCEPopup.editor.documentBaseURI.toAbsolute(u);

        if (!st)
            tinyMCEPopup.dom.setHTML('prev', '<img id="previewImg" src="' + u + '" border="0" onload="ImageDialog.updateImageData(this);" onerror="ImageDialog.resetImageData();" />');
        else
            tinyMCEPopup.dom.setHTML('prev', '<img id="previewImg" src="' + u + '" border="0" onload="ImageDialog.updateImageData(this, 1);" />');
    }
};

ImageDialog.preInit();
tinyMCEPopup.onInit.add(ImageDialog.init, ImageDialog);
