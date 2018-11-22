//
// @author daemach
//

daemach = function() {
    this.name = "Daemach's Toolbox";
    this.version = "2.0";
    this.debug = false;

    // load extensions
    this.cw = new colorWeasel(this);
    this.cm = new cssMonkey(this);
};

jQuery.extend(daemach, {
    prototype: {
        log: function() {
            if (!top.window.console || !top.window.console.log || !this.debug) {
                return;
            } else {
                top.window.console.log([].join.call(arguments, ''));
            };
        },
        time: function() {
            if (!top.window.console || !top.window.console.time || !this.debug) {
                return;
            } else {
                top.window.console.time([].join.call(arguments, ''));
            };
        },
        timeEnd: function() {
            if (!top.window.console || !top.window.console.timeEnd || !this.debug) {
                return;
            } else {
                top.window.console.timeEnd([].join.call(arguments, ''));
            };
        },
        profile: function() {
            if (!top.window.console || !top.window.console.profile || !this.debug) {
                return;
            } else {
                top.window.console.profile([].join.call(arguments, ''));
            };
        },
        profileEnd: function() {
            if (!top.window.console || !top.window.console.profileEnd || !this.debug) {
                return;
            } else {
                top.window.console.profileEnd([].join.call(arguments, ''));
            };
        },
        delay: function(condition, callback, scope, interval, done, i) {
            interval = interval || 15;
            if (typeof done === "undefined") {
                var done = false;
                i = setInterval(function() { $d.delay(condition, callback, scope, interval, done, i); }, interval)
            } else {
                var con = condition.apply(scope);
                console.log(con);
                if (con) {
                    clearInterval(i);
                    callback.call(scope);
                }
            }
        }

    }
});

//
// ==============================================================================================
// ColorWeasel v1.4
// ==============================================================================================
//

colorWeasel = function(root) {
    this.name = "colorWeasel";
    this.version = "1.4";
    this.root = root;
};
jQuery.extend(colorWeasel, {
    prototype: {
        rgb2hsl: function(rgb) {
            rgb = this.rgb2hex(rgb);
            var r = parseInt(rgb.substr(0, 2), 16) / 255;
            var g = parseInt(rgb.substr(2, 2), 16) / 255;
            var b = parseInt(rgb.substr(4, 2), 16) / 255;
            var max = Math.max(r, g, b),
				min = Math.min(r, g, b),
				delta = (max - min),
				l = (max + min) / 2,
				h = 0,
				s = 0,
				dR, dG, dB;
            if (max != min) {
                s = (l < .5) ? (max - min) / (max + min) : (max - min) / (2 - max - min);
                dR = (((max - r) / 6) + (delta / 2)) / delta;
                dG = (((max - g) / 6) + (delta / 2)) / delta;
                dB = (((max - b) / 6) + (delta / 2)) / delta;
                h = (max != r) ? (max != g) ? ((2 / 3) + dG - dR) : ((1 / 3) + dR - dB) : (dB - dG);
            };
            if (h < 0) { h += 1; };
            if (h > 1) { h -= 1; };
            h *= 360;
            return [h, s, l];
        },
        hsl2rgb: function(h, s, l) { // H as degrees 0..360, S L as decimals, 0..1.
            if (typeof h == "object" && h.constructor == Array) {
                l = h[2];
                s = h[1];
                h = h[0];
            };
            h /= 360;
            var y = (l > .5) ? (l + s) - (l * s) : l * (s + 1),
		    x = l * 2 - y,
		    r = Math.round(255 * _hue2Rgb(x, y, h + (1 / 3))),
		    g = Math.round(255 * _hue2Rgb(x, y, h)),
		    b = Math.round(255 * _hue2Rgb(x, y, h - (1 / 3)));

            function _hue2Rgb(x, y, h) {
                if (h < 0) {
                    h += 1;
                } else if (h > 1) {
                    h -= 1;
                };

                return ((h * 6) < 1) ? (x + (y - x) * h * 6) : ((h * 2) < 1) ? y : ((h * 3) < 2) ? (x + (y - x) * ((2 / 3) - h) * 6) : x;
            }
            return this.zeroPad(r.toString(16)).toUpperCase() + this.zeroPad(g.toString(16)).toUpperCase() + this.zeroPad(b.toString(16)).toUpperCase();
        },
        zeroPad: function(num) {
            var str = '0' + num;
            return str.substring(str.length - 2);
        },
        rgb2hex: function(rgb) {
            if (!rgb.match(/(rgb\()[^\)]+(\))/)) {
                return rgb;
            };
            var t = /(rgb\()([^\)]+)(\))/.exec(rgb);
            t = t[2].replace(/\s+/g, "").split(",");
            var r = this.zeroPad(parseInt(t[0]).toString(16).toUpperCase());
            var g = this.zeroPad(parseInt(t[1]).toString(16).toUpperCase());
            var b = this.zeroPad(parseInt(t[2]).toString(16).toUpperCase());

            return r + g + b;
        },
        ccLighter: function(rgb, perc) {
            var hsl = this.rgb2hsl(rgb);
            hsl[2] += (hsl[2] *= perc);
            hsl[2] = (hsl[2] >= 1) ? 1 : hsl[2];
            rgb = this.hsl2rgb(hsl);
            return rgb;
        },
        ccDarker: function(rgb, perc) {
            var hsl = this.rgb2hsl(rgb);
            hsl[2] -= (hsl[2] *= perc);
            hsl[2] = (hsl[2] <= 0) ? 0 : hsl[2];
            rgb = this.hsl2rgb(hsl);
            return rgb;
        },
        ccComplementary: function(rgb) {
            var hsl = this.rgb2hsl(rgb);
            hsl[0] += 180;
            hsl[0] = (hsl[0] > 360) ? hsl[0] - 360 : hsl[0];
            rgb = this.hsl2rgb(hsl);
            return rgb;
        }
    }
});



//
// ==============================================================================================
// CSSMonkey v1.2
// ==============================================================================================
//

//
// @author daemach
//

cssMonkey = function(root) {
    this.name = "cssMonkey";
    this.version = "1.2";
    this.root = root;
    this.sheets = [];
    if (document.styleSheets) {
        this.parseStyles();
    } else {
        return false;
    }
};

jQuery.extend(cssMonkey, {
    prototype: {
        parseStyles: function() {
            var media, mediaType, styleSheet;
            if (document.styleSheets.length > 0) {
                for (var i = 0; i < document.styleSheets.length; i++) {
                    if (document.styleSheets[i].disabled) {
                        continue;
                    }
                    media = document.styleSheets[i].media;
                    mediaType = typeof media;

                    if (mediaType == "string") {
                        if (media == "" || media.indexOf("screen") != -1) {
                            styleSheet = document.styleSheets[i];
                        }
                    } else if (mediaType == "object") {
                        if (media.mediaText == "" || media.mediaText.indexOf("screen") != -1) {
                            styleSheet = document.styleSheets[i];
                        }
                    }
                    if (typeof styleSheet != "undefined") {
                        this.sheets.push(styleSheet);
                    }
                }
            }
        },
        toggleSheet: function(disable, id) {
            var s = this.sheets;
            id = (typeof id === "undefined") ? null : id;
            disable = (typeof disable === "undefined") ? null : disable;

            for (var i = 0; i < s.length; i++) {
                if (id === null || i == id || s[i].href.indexOf(id) >= 0) {
                    s[i].disabled = (disable || (disable === null && !s[i].disabled)) ? true : false;
                }
            }
        },
        getRule: function(s, a) {
            var rules, matches = [], sObj = [];
            for (var i = this.sheets.length - 1; i >= 0; i--) {
                rules = (this.sheets[i].cssRules) ? this.sheets[i].cssRules : this.sheets[i].rules;
                if (!rules.length) {
                    return matches;
                }
                s = s.toLowerCase();
                for (var r = rules.length - 1; r >= 0; r--) {
                    if (typeof rules[r].selectorText != "undefined" && rules[r].selectorText.toLowerCase() == s) {
                        a = (typeof a == "undefined") ? null : rules[r].style[this.camelCase(a)];
                        matches.push([rules[r], i, r, a]);
                    }
                }
            }
            return matches;
        },
        _findStyle: function(cssText, attr) {
            var n = (attr.indexOf(":") >= 0) ? attr.split(":")[0].trim() : attr;
            if (n.length) {
                for (var i = 0; i < cssText.length; i++) {
                    if (cssText[i].indexOf(n) >= 0) { return i; }
                }
            }
            return -1;
        },
        camelCase: function(s) {
            if (s == "float") { return "cssFloat"; };
            for (var str = /-([a-z])/; str.test(s); s = s.replace(str, RegExp.$1.toUpperCase()));
            return s;
        },
        getComputedStyle: function(ele, attr) {
            attr = this.camelCase(attr);
            if (ele.currentStyle) {
                return ele.currentStyle[attr];
            } else if (window.getComputedStyle) {
                return window.getComputedStyle(ele, null)[attr];
            }
            return null;
        },
        getRootStyle: function(ele, attr) {
            attr = this.camelCase(attr);
            var tmpAttr = this.getComputedStyle(ele, attr), tmpEle = ele;
            if (attr.indexOf("Color") >= 0) {
                tmpAttr = this.root.cw.rgb2hex(tmpAttr);
                while (tmpEle !== "body" && tmpAttr == "transparent") {
                    tmpEle = tmpEle.parentNode;
                    tmpAttr = this.root.cw.rgb2hex(this.getComputedStyle(tmpEle, attr));
                }
                tmpAttr = (tmpAttr == "transparent") ? "000000" : tmpAttr;
            }
            return tmpAttr;
        },
        setRule: function(selector, styles, index) {

            var rules = this.getRule(selector), oCSS, nCSS, o, s, rCSS;
            if (rules.length) {
                nCSS = styles.split(";");
                if (!nCSS[nCSS.length - 1].trim().length) { nCSS.pop(); }
                for (var i = 0; i < rules.length; i++) {
                    oCSS = rules[i][0].style.cssText.split(";");
                    for (var x = 0; x < nCSS.length; x++) {
                        s = this._findStyle(oCSS, nCSS[x]);
                        if (s >= 0) {
                            oCSS[s] = nCSS[x];
                        } else {
                            oCSS.push(nCSS[x]);
                        }
                    }
                    rules[i][0].style.cssText = oCSS.join(";");
                }
            } else {
                var sheet = this.sheets[this.sheets.length - 1];
                var rules = sheet.cssRules ? sheet.cssRules : sheet.rules;
                if (index == undefined) {
                    index = rules.length;
                }
                this._insertRule(sheet, selector, styles, index)
            }
        },
        _insertRule: function(sheet, selector, styles, index) {

            if (sheet.insertRule) {
                sheet.insertRule(selector + "{" + styles + "}", index);
            } else if (sheet.addRule) {
                sheet.addRule(selector, styles, index);
            }
        },
        deleteRule: function(s) {
            var rules = this.getRule(s), sheet;
            for (var i = rules.length - 1; i >= 0; i--) {
                sheet = this.sheets[rules[i][1]];
                if (sheet.deleteRule) {
                    sheet.deleteRule(rules[i][2]);
                } else if (sheet.removeRule) {
                    sheet.removeRule(rules[i][2]);
                }
            }
        }
    }
});









//
// ==============================================================================================
// Light it up...
// ==============================================================================================
//
if (typeof $daemach == "undefined") {
    var $daemach = new daemach();
}
var $d = $daemach;


