/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference name="MicrosoftAjax.js"/>
/// <reference path="/umbraco_client/Application/JQuery/jquery.ba-bbq.min.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {

    Umbraco.Controls.HistoryManager = function() {
        /// <summary>This is a wrapper for the bbq plugin history manager, but we could do alot with history mgmt in the future!</summary>

        var hashFragmentRegex = new RegExp(/^\w+/);
        function getHashFragment(frag) {
            //tests for xss and ensures only the first alphanumeric chars are matched
            var result = hashFragmentRegex.exec(frag);
            if (result != null && result.length > 0) {
                return result[0];
            }
            return "";
        }
        var obj = {

            onNavigate: function(e) {
                var fragment = getHashFragment($.param.fragment());
                if (fragment != "") {
                    $(window.top).trigger("navigating", [fragment]); //raise event!   
                }
                
            },
            addHistory: function(name, forceRefresh) {
                var fragment = getHashFragment($.param.fragment());
                if (fragment == name && forceRefresh) {
                    this.onNavigate();
                }
                else {
                    $.bbq.pushState(name, 2);
                }

            },
            getCurrent: function () {                
                return getHashFragment($.param.fragment());
            },

            addEventHandler: function(fnName, fn) {
                /// <summary>Adds an event listener to the event name event</summary>
                if (typeof ($) != "undefined") $(window.top).bind(fnName, fn); //if there's no jQuery, there is no events
            },
            removeEventHandler: function(fnName, fn) {
                /// <summary>Removes an event listener to the event name event</summary>
                if (typeof ($) != "undefined") $(window.top).unbind(fnName, fn); //if there's no jQuery, there is no events
            }
        };

        //wire up the navigate events, wrap method to maintain scope
        $(window).bind('hashchange', function(e) { obj.onNavigate.call(obj); });

        return obj;
    };

})(jQuery);