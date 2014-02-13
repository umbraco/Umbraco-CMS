//
// password_strength_plugin.js
// Copyright (c) 2009 myPocket technologies (www.mypocket-technologies.com)
 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// View the GNU General Public License <http://www.gnu.org/licenses/>.

// @author Darren Mason (djmason9@gmail.com)
// @date 1/23/2009
// @projectDescription Password Strength Meter is a jQuery plug-in provide you smart algorithm to detect a password strength. Based on Firas Kassem orginal plugin - http://phiras.wordpress.com/2007/04/08/password-strength-meter-a-jquery-plugin/
// @version 1.0.0
// 
// @requires jquery.js (tested with 1.3.1)
// @param shortPass:	"shortPass",	//optional
// @param badPass:		"badPass",		//optional
// @param goodPass:		"goodPass",		//optional
// @param strongPass:	"strongPass",	//optional
// @param baseStyle:	"testresult",	//optional
// @param userid:		"",				//required override
// @param messageloc:	1				//before == 0 or after == 1
//


(function ($) {
    $.fn.shortPass = 'The password is too short';
    $.fn.badPass = 'The password is weak';
    $.fn.goodPass = 'Good password';
    $.fn.strongPass = 'Strong password';
    $.fn.samePassword = 'User name and Password are identical.';
    $.fn.resultStyle = "";

    $.fn.passStrength = function (options) {

        var defaults = {
            shortPass: 'shortPass', //optional
            badPass: 'badPass', 	//optional
            goodPass: 'goodPass', 	//optional
            strongPass: 'strongPass', //optional
            baseStyle: 'testresult', //optional
            userid: '', 			//required override
            messageloc: 1,				//before == 0 or after == 1
            minLength: 7
        };
        var opts = $.extend(defaults, options);

        return this.each(function () {
            var obj = $(this);

            $(obj).unbind().keyup(function () {

                var results = $.fn.teststrength($(this).val(), opts.userid, opts);
                var fieldSpan = this.parent;

                if (opts.messageloc === 1) {
                    $(this).parent().next("." + opts.baseStyle).remove();
                    $(this).parent().after("<span class=\"" + opts.baseStyle + "\"><strong></strong></span>");
                    $(this).parent().next("." + opts.baseStyle).addClass($(this).resultStyle).find("strong").text(results);
                }
                else {
                    $(this).prev("." + opts.baseStyle).remove();
                    $(this).before("<span class=\"" + opts.baseStyle + "\"><strong></strong></span>");
                    $(this).prev("." + opts.baseStyle).addClass($(this).resultStyle).find("strong").text(results);
                }
            });

            //FUNCTIONS
            $.fn.teststrength = function (password, username, option) {
                var score = 0;

                //password < minLength
                if (password.length < option.minLength) { this.resultStyle = option.shortPass; return $(this).shortPass; }

                //password == user name
                if (password.toLowerCase() == username.toLowerCase()) { this.resultStyle = option.badPass; return $(this).samePassword; }

                //password length
                score += password.length // 4;
                score += ($.fn.checkRepetition(1, password).length - password.length) // 1;
                score += ($.fn.checkRepetition(2, password).length - password.length) // 1;
                score += ($.fn.checkRepetition(3, password).length - password.length) // 1;
                score += ($.fn.checkRepetition(4, password).length - password.length) // 1;

                //password has 3 numbers
                if (password.match(/(.*[0-9].*[0-9].*[0-9])/)) { score += 5; }

                //password has 2 symbols
                if (password.match(/(.*[!,@,#,$,%,^,&,*,?,_,~].*[!,@,#,$,%,^,&,*,?,_,~])/)) { score += 5; }

                //password has Upper and Lower chars
                if (password.match(/([a-z].*[A-Z])|([A-Z].*[a-z])/)) { score += 10; }

                //password has number and chars
                if (password.match(/([a-zA-Z])/) && password.match(/([0-9])/)) { score += 15; }
                //
                //password has number and symbol
                if (password.match(/([!,@,#,$,%,^,&,*,?,_,~])/) && password.match(/([0-9])/)) { score += 15; }

                //password has char and symbol
                if (password.match(/([!,@,#,$,%,^,&,*,?,_,~])/) && password.match(/([a-zA-Z])/)) { score += 15; }

                //password is just a numbers or chars
                if (password.match(/^\w+$/) || password.match(/^\d+$/)) { score -= 10; }

                //verifying 0 < score < 100
                if (score < 0) { score = 0; }
                if (score > 100) { score = 100; }

                if (score < 34) { this.resultStyle = option.badPass; return $(this).badPass; }
                if (score < 68) { this.resultStyle = option.goodPass; return $(this).goodPass; }

                this.resultStyle = option.strongPass;
                return $(this).strongPass;

            };

        });
    };
})(jQuery);


$.fn.checkRepetition = function(pLen, str) {
    var res = '';
    for (var i = 0; i < str.length; i++) {
        var repeated = true;

        for (var j = 0; j < pLen && (j + i + pLen) < str.length; j++) {
            repeated = repeated && (str.charAt(j + i) == str.charAt(j + i + pLen));
        }
        if (j < pLen) { repeated = false; }
        if (repeated) {
            i += pLen - 1;
            repeated = false;
        }
        else {
            res += str.charAt(i);
        }
    }
    return res;
};
