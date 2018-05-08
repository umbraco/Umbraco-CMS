(function ($) {

    //extensions to base classes such as String and extension methods for jquery.
    //NOTE: jquery must be loaded before this file.

    //create guid object on the window (which makes it global)
    if (window.Guid == null) {
        window.Guid = {
            generate: function () {
                ///<summary>generates a new Guid</summary>
                return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                    return v.toString(16);
                });
            }
        };
    }

    if (!window.__debug__) {
        window.__debug__ = function (msg, category, isErr) {
            ///<summary>global method to send debug statements to console that is cross browser (or at least checks if its possible)</summary>        

            if (((typeof console) != "undefined") && console.log && console.error) {
                if (isErr) console.error(category + ": " + msg);
                else console.log(category + ": " + msg);
            }
        };
    }

    if (!window.location.getParams) {
      var pl = /\+/g;  // Regex for replacing addition symbol with a space
      var search = /([^&=]+)=?([^&]*)/g;
      var decode = function(s) { return decodeURIComponent(s.replace(pl, " ")); };
      
      window.location.getParams = function() {
        var match;
        var query = window.location.search.substring(1);

        var urlParams = {};
        while (match = search.exec(query))
          urlParams[decode(match[1])] = decode(match[2]); 

        return urlParams;
      }
    }
    
    if (!String.prototype.startsWith) {
        String.prototype.startsWith = function (str) {
            ///<summary>startsWith extension method for string</summary>

            return this.substr(0, str.length) === str;
        };
    }

    if (!String.prototype.endsWith) {
        String.prototype.endsWith = function (str) {
            ///<summary>endsWith extension method for string</summary>

            return this.substr(this.length - str.length) === str;
        };
    }

    if (!String.prototype.utf8Encode) {
        String.prototype.utf8Encode = function () {
            ///<summary>UTF8 encoder for string</summary>

            var str = this.replace(/\r\n/g, "\n");
            var utftext = "";
            for (var n = 0; n < str.length; n++) {
                var c = str.charCodeAt(n);
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
        };
    }

    if (!String.prototype.utf8Decode) {
        String.prototype.utf8Decode = function () {
            var utftext = this;
            var string = "";
            var i = 0;
            var c = c1 = c2 = 0;

            while (i < utftext.length) {

                c = utftext.charCodeAt(i);

                if (c < 128) {
                    string += String.fromCharCode(c);
                    i++;
                }
                else if ((c > 191) && (c < 224)) {
                    c2 = utftext.charCodeAt(i + 1);
                    string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                    i += 2;
                }
                else {
                    c2 = utftext.charCodeAt(i + 1);
                    c3 = utftext.charCodeAt(i + 2);
                    string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                    i += 3;
                }

            }

            return string;
        };
    }

    if (!String.prototype.base64Encode) {
        String.prototype.base64Encode = function () {
            ///<summary>Base64 encoder for string</summary>

            var keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            var output = "";
            var chr1, chr2, chr3, enc1, enc2, enc3, enc4;
            var i = 0;

            var input = this.utf8Encode();

            while (i < input.length) {

                chr1 = input.charCodeAt(i++);
                chr2 = input.charCodeAt(i++);
                chr3 = input.charCodeAt(i++);

                enc1 = chr1 >> 2;
                enc2 = ((chr1 & 3) << 4) | (chr2 >> 4);
                enc3 = ((chr2 & 15) << 2) | (chr3 >> 6);
                enc4 = chr3 & 63;

                if (isNaN(chr2)) {
                    enc3 = enc4 = 64;
                } else if (isNaN(chr3)) {
                    enc4 = 64;
                }

                output = output +
			        keyStr.charAt(enc1) + keyStr.charAt(enc2) +
			        keyStr.charAt(enc3) + keyStr.charAt(enc4);

            }

            return output;
        };
    }

    if (!String.prototype.base64Decode) {
        String.prototype.base64Decode = function () {
            ///<summary>Base64 decoder for string</summary>

            var input = this;
            var keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            var output = "";
            var chr1, chr2, chr3;
            var enc1, enc2, enc3, enc4;
            var i = 0;

            input = input.replace(/[^A-Za-z0-9\+\/\=]/g, "");

            while (i < input.length) {

                enc1 = keyStr.indexOf(input.charAt(i++));
                enc2 = keyStr.indexOf(input.charAt(i++));
                enc3 = keyStr.indexOf(input.charAt(i++));
                enc4 = keyStr.indexOf(input.charAt(i++));

                chr1 = (enc1 << 2) | (enc2 >> 4);
                chr2 = ((enc2 & 15) << 4) | (enc3 >> 2);
                chr3 = ((enc3 & 3) << 6) | enc4;

                output = output + String.fromCharCode(chr1);

                if (enc3 != 64) {
                    output = output + String.fromCharCode(chr2);
                }
                if (enc4 != 64) {
                    output = output + String.fromCharCode(chr3);
                }

            }

            return output.utf8Decode();

        };
    }


    if (!Math.randomRange) {
        Math.randomRange = function (from, to) {
            ///<summary>randomRange extension for math</summary>

            return Math.floor(Math.random() * (to - from + 1) + from);
        };
    }

    if (!String.prototype.toCamelCase) {
        String.prototype.toCamelCase = function () {
            ///<summary>toCamelCase extension method for string</summary>

            var s = this.toPascalCase();
            if ($.trim(s) == "")
                return "";
            if (s.length > 1) {
                var regex = /^([A-Z]*)([A-Z].*)/g;
                if (s.match(regex)) {
                    var match = regex.exec(s);
                    s = match[1].toLowerCase() + match[2];
                    s = s.substr(0, 1).toLowerCase() + s.substr(1);
                }
            } else {
                s = s.toLowerCase();
            }
            return s;
        };
    }

    if (!String.prototype.toPascalCase) {
        String.prototype.toPascalCase = function () {
            ///<summary>toPascalCase extension method for string</summary>

            var s = "";
            $.each($.trim(this).split(/[\s\.-]+/g), function (idx, val) {
                if ($.trim(val) == "")
                    return;
                if (val.length > 1)
                    s += val.substr(0, 1).toUpperCase() + val.substr(1);
                else
                    s += val.toUpperCase();
            });
            return s;
        };
    }

    if (!String.prototype.toUmbracoAlias) {
        String.prototype.toUmbracoAlias = function () {
            ///<summary>///<summary>toUmbracoAlias extension method for string</summary></summary>

            var s = this.replace(/[^a-zA-Z0-9\s\.-]+/g, ''); // Strip none alphanumeric chars
            return s.toCamelCase(); // Convert to camelCase
        };
    }

    if (!String.prototype.toFunction) {
        String.prototype.toFunction = function () {
            var arr = this.split(".");
            var fn = (window || this);
            for (var i = 0, len = arr.length; i < len; i++) {
                fn = fn[arr[i]];
            }
            if (typeof fn !== "function") {
                throw new Error("function not found");
            }
            return fn;
        };
    }

    //sets defaults for ajax
    $.ajaxSetup({
        dataType: 'json',
        cache: false,
        contentType: 'application/json; charset=utf-8',
        error: function (x, t, e) {
            if (x.status.toString().startsWith("500")) {
                //show ysod overlay if we can
                if (UmbClientMgr) {
                    var startIndex = x.responseText.indexOf("<body");
                    var endIndex = x.responseText.lastIndexOf("</body>");
                    var body = x.responseText.substring(startIndex, endIndex + 7);
                    var $div = $(body.replace("<body bgcolor=\"white\">", "<div style='display:none;overflow:auto;height:613px;'>").replace("</body>", "</div>"));
                    $div.appendTo($(UmbClientMgr.mainWindow().document.getElementsByTagName("body")[0]));
                    UmbClientMgr.openModalWindowForContent($div, "ysod", true, 640, 640, null, null, null, function() {
                        //remove the $div
                        $div.closest(".umbModalBox").remove();
                    });
                }
                else {
                    alert("Unhandled exception occurred.\nStatus: " + x.status + "\nMessage: " + x.statusText + "\n\n" + x.responseText);
                }
            }
        }
    });

    $.fn.getAllAttributes = function () {
        ///<summary>extension method to get all attributes of a selected element</summary>

        if ($(this).length != 1) {
            throw "the getAllAttributes method can only be called when matching one jQuery selector";
        };
        var el = $(this).get(0);
        var arr = [];
        for (var i = 0, attrs = el.attributes; i < attrs.length; i++) {
            arr.push({ name: attrs.item(i).nodeName, value: attrs.item(i).nodeValue });
        }
        return arr;
    };

    $.fn.outerHtml = function () {
        ///<summary>extension to get the 'outer html' of an element</summary>

        if ($(this).length != 1) {
            throw "the getAllAttributes method can only be called when matching one jQuery selector";
        };
        var nodeName = $(_opts.content).get(0).nodeName.toLowerCase();
        //start creating raw html
        var outerHtml = "<" + nodeName;
        //get all the attributes/values from the original element and add them to the new one
        var allAttributes = $(_opts.content).getAllAttributes();
        for (var a in allAttributes) {
            outerHtml += " " + allAttributes[a].name + "='" + allAttributes[a].value + "'";
        }
        outerHtml += ">";
        outerHtml += $(_opts.content).html();
        outerHtml += "</" + nodeName + ">";
        return outerHtml;
    };

    $.fn.focusFirst = function () {
        ///<summary>extension to focus the first editable field in a form</summary>

        return $(this).each(function () {
            if ($(this).get(0).nodeName.toLowerCase() != "form") {
                throw "The focusFirst method can only be applied to a form element";
            }
            var first = $(this).find(":input:enabled:visible").not(":submit").not(":button").not(":file").not(":image").not(":radio");
            if (first.length > 0) {
                $(first[0]).focus();
            }
        });
    };

    $.fn.getAttributes = function () {
        ///<summary>Extension method to return all of the attributes for an element</summary>

        var attributes = [];

        if (!this.length)
            return this;

        $.each(this[0].attributes, function (index, attr) {
            attributes.push({ name: attr.name, value: attr.value });
        });

        return attributes;
    };


    //defaults that need to be set on ready
    $(document).ready(function () {

        //adds a default ignore parameter to jquery validation
        if ($.validator) {
            $.validator.setDefaults({ ignore: ".ignore" });
        }

        //adds a "re-parse" method to the Unobtrusive JS framework since Parse doesn't actually reparse
        if ($.validator && $.validator.unobtrusive) {
            $.validator.unobtrusive.reParse = function ($selector) {
                $selector.removeData("validator");
                $.validator.unobtrusive.parse($selector);
            };
        }

    });

    //This sets the default jquery ajax headers to include our csrf token, we
    // need to user the beforeSend method because our token changes per user/login so
    // it cannot be static
    $.ajaxSetup({
        beforeSend: function (xhr) {

            function getCookie(name) {
                var value = "; " + document.cookie;
                var parts = value.split("; " + name + "=");
                if (parts.length === 2)
                  return parts.pop().split(";").shift();
                return null;
            }

            var cookieVal = getCookie("UMB-XSRF-TOKEN");
            if (cookieVal) {
              xhr.setRequestHeader("X-UMB-XSRF-TOKEN", cookieVal);  
            }

            var queryString = window.location.getParams();
            if (queryString.umbDebug === "true") {
              xhr.setRequestHeader("X-UMB-DEBUG", cookieVal);  
            }
        }
    });

})(jQuery);