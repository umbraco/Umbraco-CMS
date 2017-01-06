(function () {

    //JavaScript extension methods on the core JavaScript objects (like String, Date, etc...)
    if (!Date.prototype.toIsoDateTimeString) {
        /** Converts a Date object to a globally acceptable ISO string, NOTE: This is different from the built in 
            JavaScript toISOString method which returns date/time like "2013-08-07T02:04:11.487Z" but we want "yyyy-MM-dd HH:mm:ss" */
        Date.prototype.toIsoDateTimeString = function (str) {
            var month = (this.getMonth() + 1).toString();
            if (month.length === 1) {
                month = "0" + month;
            }
            var day = this.getDate().toString();
            if (day.length === 1) {
                day = "0" + day;
            }
            var hour = this.getHours().toString();
            if (hour.length === 1) {
                hour = "0" + hour;
            }
            var mins = this.getMinutes().toString();
            if (mins.length === 1) {
                mins = "0" + mins;
            }
            var secs = this.getSeconds().toString();
            if (secs.length === 1) {
                secs = "0" + secs;
            }
            return this.getFullYear() + "-" + month + "-" + day + " " + hour + ":" + mins + ":" + secs;
        };
    }
    
    if (!Date.prototype.toIsoDateString) {
        /** Converts a Date object to a globally acceptable ISO string, NOTE: This is different from the built in 
            JavaScript toISOString method which returns date/time like "2013-08-07T02:04:11.487Z" but we want "yyyy-MM-dd" */
        Date.prototype.toIsoDateString = function (str) {
            var month = (this.getMonth() + 1).toString();
            if (month.length === 1) {
                month = "0" + month;
            }
            var day = this.getDate().toString();
            if (day.length === 1) {
                day = "0" + day;
            }
            
            return this.getFullYear() + "-" + month + "-" + day;
        };
    }

    //create guid method on the String
    if (String.CreateGuid == null) {
        
        /** generates a new Guid */
        String.CreateGuid = function () {            
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                return v.toString(16);
            });
        };
    }

    if (!window.__debug__) {
        
        /** global method to send debug statements to console that is cross browser (or at least checks if its possible)*/
        window.__debug__ = function (msg, category, isErr) {
            if (((typeof console) != "undefined") && console.log && console.error) {
                if (isErr) console.error(category + ": " + msg);
                else console.log(category + ": " + msg);
            }
        };
    }

    if (!String.prototype.startsWith) {
        /** startsWith extension method for string */
        String.prototype.startsWith = function (str) {            
            return this.substr(0, str.length) === str;
        };
    }

    if (!String.prototype.endsWith) {
        
        /** endsWith extension method for string*/
        String.prototype.endsWith = function (str) {            
            return this.substr(this.length - str.length) === str;
        };
    }
    
    if (!String.prototype.trimStart) {
        
        /** trims the start of the string*/
        String.prototype.trimStart = function (str) {
            if (this.startsWith(str)) {
                return this.substring(str.length);
            }
            return this;
        };
    }
    
    if (!String.prototype.trimEnd) {

        /** trims the end of the string*/
        String.prototype.trimEnd = function (str) {
            if (this.endsWith(str)) {
                return this.substring(0, this.length - str.length);
            }
            return this;
        };
    }

    if (!String.prototype.utf8Encode) {

        /** UTF8 encoder for string*/
        String.prototype.utf8Encode = function () {
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
        
        /** UTF8 decoder for string*/
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
        
        /** Base64 encoder for string*/
        String.prototype.base64Encode = function () {
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
        
        /** Base64 decoder for string*/
        String.prototype.base64Decode = function () {

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
        
        /** randomRange extension for math*/
        Math.randomRange = function (from, to) {
            return Math.floor(Math.random() * (to - from + 1) + from);
        };
    }

    if (!String.prototype.toCamelCase) {
        
        /** toCamelCase extension method for string*/
        String.prototype.toCamelCase = function () {

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
        
        /** toPascalCase extension method for string*/
        String.prototype.toPascalCase = function () {
            
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
        
        /** toUmbracoAlias extension method for string*/
        String.prototype.toUmbracoAlias = function () {
            
            var s = this.replace(/[^a-zA-Z0-9\s\.-]+/g, ''); // Strip none alphanumeric chars
            return s.toCamelCase(); // Convert to camelCase
        };
    }

    if (!String.prototype.toFunction) {
        
        /** Converts a string into a function if it is found */
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

    if (!String.prototype.detectIsJson) {

        /** Rudimentary check to see if the string is a json encoded string */
        String.prototype.detectIsJson = function () {
            if ((this.startsWith("{") && this.endsWith("}")) || (this.startsWith("[") || this.endsWith("]"))) {
                return true;
            }
            return false;
        };
    }


})();