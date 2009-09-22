



// Ctrl + S support
var ctrlDown = false;
var shiftDown = false;
var keycode = 0

function shortcutCheckKeysDown(e) {

    ctrlDown = e.ctrlKey;
    shiftDown = e.shiftKey;
    keycode = e.keyCode;

    //save
    if (ctrlDown && keycode == 83) {
        doSubmit();
        if (window.addEventListener) {
            e.preventDefault();
        } else
            return false;
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
    $: function(id) {
        return document.getElementById(id);
    },

    watch: function(obj) {
        if (obj && this.$(obj)) {
            this.watching["_" + obj] = this.$(obj);
            this.addEvent(this.$(obj), "keydown", function(evt) {
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

    click: function(obj, fn) {
        if (obj && this.$(obj)) {
            this.addEvent(this.$(obj), "click", function() {
                tab.results["_" + this.id.split("_")[1]] = tab.parse(tab.watching["_" + this.id.split("_")[1]].value);

                if (fn && fn.constructor == Function) {
                    fn();
                }
            });
        }
    },

    get: function(obj) {
        if (obj && this.$(obj)) {
            return this.results["_" + obj];
        }
    },

    parse: function(str) {
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

    addEvent: function(obj, type, fn) {
        if (obj.attachEvent) {
            obj["e" + type + fn] = fn;
            obj[type + fn] = function() {
                obj["e" + type + fn](window.event);
            }

            obj.attachEvent("on" + type, obj[type + fn]);
        } else {
            obj.addEventListener(type, fn, false);
        }
    }
};


//////////////////////////////////////
// CARET funktioner
//////////////////////////////////////
var tempCaretEl;
function storeCaret(editEl) {
    if (editEl.createTextRange) {
        editEl.currRange = document.selection.createRange().duplicate();
    }
}

function setCaretToEnd(el) {
    if (el.createTextRange) {
        var v = el.value;
        var r = el.createTextRange();
        r.moveStart('character', v.length);
        r.select();
    }
}

function insertAtEnd(el, txt) {
    txt = caretTextUnencode(txt);
    el.value += txt;
    setCaretToEnd(el);
}

function insertAtCaret(el, txt) {
    txt = caretTextUnencode(txt);
    if (el.currRange) {
        el.currRange.text =
				el.currRange.text.charAt(el.currRange.text.length - 1) != ' ' ? txt :
			txt + ' ';
        el.currRange.select();

    }
    else
        insertAtEnd(el, txt);
}

function insertAtCaretAndMove(el, txt, move) {
    txt = caretTextUnencode(txt);
    if (el.currRange) {
        el.currRange.text = el.currRange.text.charAt(el.currRange.text.length - 1) != ' ' ? txt : txt + ' ';
        el.currRange.moveStart('character', move);
        el.currRange.moveEnd('character', move);
        el.currRange.select();
    }
    else
        insertAtEnd(el, txt);
}

function caretTextUnencode(txt) {
    return txt; //.replace(/\&quot;/gi,"\"")
}
