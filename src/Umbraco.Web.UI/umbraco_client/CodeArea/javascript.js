function resizeTextArea(textEditor, offsetX, offsetY) {
    var clientHeight = getViewportHeight();
    var clientWidth = getViewportWidth();

    if (textEditor != null) {
       // textEditor.style.width = (clientWidth - offsetX) + "px";
        textEditor.style.height = (clientHeight - getY(textEditor) - offsetY) + "px";
    }
}

var currentHandle = null, currentLine;
function updateLineInfo(cm) {

    var line = cm.getCursor().line, handle = cm.getLineHandle(line);
    if (handle == currentHandle && line == currentLine) return;

    if (currentHandle) {
        cm.setLineClass(currentHandle, null, null);
    //    cm.clearMarker(currentHandle);
    }

    currentHandle = handle; currentLine = line;
    cm.setLineClass(currentHandle, null, "activeline");
    //cm.setMarker(currentHandle, String(line + 1));
} 


function UmbracoCodeSnippet() {
    this.BeginTag = "";
    this.EndTag = "";
    this.TargetId = "";
    this.CursorPos = 0;
}


// Ctrl + S support
var ctrlDown = false;
var shiftDown = false;
var keycode = 0

function shortcutCheckKeysDown(e) {

    ctrlDown = e.ctrlKey;
    shiftDown = e.shiftKey;
    keycode = e.keyCode;

    //save
    // uncommented by NH 07-05-11 as it's been replaced by a native bindShortcutkey() method in the ClientManager
/*
    if (ctrlDown && keycode == 83) {
        doSubmit();
        if (window.addEventListener) {
            e.preventDefault();
        } else
            return false;
    }
    */
    //snippet
    if (ctrlDown && keycode == 77) {
        if (window.umbracoInsertSnippet) {
            var snippetCode = umbracoInsertSnippet();
            if (window.UmbEditor) {
                UmbEditor.Insert(snippetCode.BeginTag, snippetCode.EndTag, snippetCode.TargetId);
                if (window.addEventListener) {
                    e.preventDefault();
                } else
                    return false;
            }
        }

    }

    //load the insert value dialog: ctrl + g
    if (ctrlDown && keycode == 71) {
        umbracoInsertField('', 'xsltInsertValueOf', '', 'felt', 750, 230, '');
        if (window.addEventListener) {
            e.preventDefault();
        } else
            return false;
    }
}

function shortcutCheckKeysUp(e) {
    ctrlDown = e.ctrlKey;
    shiftDown = e.shiftKey;
}

function shortcutCheckKeysPressFirefox(e) {
    if (ctrlDown && keycode == 83)
        e.preventDefault();
}

if (window.addEventListener) {
    document.addEventListener('keyup', shortcutCheckKeysUp, false);
    document.addEventListener('keydown', shortcutCheckKeysDown, false);
    document.addEventListener('keypress', shortcutCheckKeysPressFirefox, false);
} else {
    document.attachEvent("onkeyup", shortcutCheckKeysUp);
    document.attachEvent("onkeydown", shortcutCheckKeysDown);
}


var tab = {
    key: 9,
    string: "\t",
    nl2br: true,
    tosp: true,
    watching: {},
    results: {},
    $: function (id) {
        return document.getElementById(id);
    },

    watch: function (obj) {
        if (obj && this.$(obj)) {
            this.watching["_" + obj] = this.$(obj);
            this.addEvent(this.$(obj), "keydown", function (evt) {
                var sct = tab.$(obj).scrollTop;
                var l = tab.$(obj).value.length;
                var evt = (evt) ? evt : ((window.event) ? event : null);

                if (evt) {
                    var elem = (evt.target) ? evt.target : ((evt.srcElement) ? evt.srcElement : null);

                    if (elem) {
                        var char_code = (evt.charCode) ? evt.charCode : ((evt.which) ? evt.which : evt.keyCode);

                        if (char_code == tab.key) {
                            if (tab.$(obj).attachEvent) {
                                var range = document.selection.createRange();

                                range.text = tab.string;
                                range.moveStart("character", -1);
                                //range.select();
                            } else if (typeof tab.$(obj).selectionStart != "undefined") {
                                var start = tab.$(obj).value.substr(0, tab.$(obj).selectionStart);
                                var end = tab.$(obj).value.substr(tab.$(obj).selectionStart, l);
                                var selection = tab.$(obj).value.replace(start, "").replace(end, "")
                                tab.$(obj).value = start + tab.string + selection + end;
                                tab.$(obj).setSelectionRange(start.length + 1, start.length + 1);
                                tab.$(obj).scrollTop = sct;
                            } else {
                                tab.$(obj).value += tab.string;
                            }

                            if (evt.preventDefault) {
                                evt.preventDefault();
                                evt.stopPropagation();
                            } else {
                                evt.returnValue = false;
                                evt.cancelBubble = true;
                            }

                            return false;
                        }
                    }
                }
            });
        }
    },

    click: function (obj, fn) {
        if (obj && this.$(obj)) {
            this.addEvent(this.$(obj), "click", function () {
                tab.results["_" + this.id.split("_")[1]] = tab.parse(tab.watching["_" + this.id.split("_")[1]].value);

                if (fn && fn.constructor == Function) {
                    fn();
                }
            });
        }
    },

    get: function (obj) {
        if (obj && this.$(obj)) {
            return this.results["_" + obj];
        }
    },

    parse: function (str) {
        var str = (str) ? str : "";

        if (str.length) {
            if (this.tosp) {
                str = str.replace(/\t/g, "&nbsp;&nbsp;&nbsp;");
            }

            if (this.nl2br) {
                str = str.replace(/\r?\n/g, "<br />");
            }
        }

        return str;
    },

    addEvent: function (obj, type, fn) {
        if (obj.attachEvent) {
            obj["e" + type + fn] = fn;
            obj[type + fn] = function () {
                obj["e" + type + fn](window.event);
            }

            obj.attachEvent("on" + type, obj[type + fn]);
        } else {
            obj.addEventListener(type, fn, false);
        }
    }
};