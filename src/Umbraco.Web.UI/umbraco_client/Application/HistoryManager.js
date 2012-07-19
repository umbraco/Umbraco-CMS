/// <reference path="/umbraco_client/Application/NamespaceManager.js" />
/// <reference name="MicrosoftAjax.js"/>
/// <reference path="/umbraco_client/Application/Query/jquery.ba-bbq.min.js" />

Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function($) {

    Umbraco.Controls.HistoryManager = function() {
        /// <summary>This is a wrapper for the bbq plugin history manager, but we could do alot with history mgmt in the future!</summary>
        var obj = {

            onNavigate: function(e) {
                
                var l = $.param.fragment();
                if (l != "") {
                    jQuery(window.top).trigger("navigating", [$.param.fragment()]); //raise event!
                }
                
            },
            addHistory: function(name, forceRefresh) {
                if ($.param.fragment() == name && forceRefresh) {
                    this.onNavigate();
                }
                else {
                    $.bbq.pushState(name, 2);
                }

            },
            getCurrent: function() {
                return ($.param.fragment().length > 0) ? $.param.fragment() : "";
            },

            addEventHandler: function(fnName, fn) {
                /// <summary>Adds an event listener to the event name event</summary>
                if (typeof (jQuery) != "undefined") jQuery(window.top).bind(fnName, fn); //if there's no jQuery, there is no events
            },
            removeEventHandler: function(fnName, fn) {
                /// <summary>Removes an event listener to the event name event</summary>
                if (typeof (jQuery) != "undefined") jQuery(window.top).unbind(fnName, fn); //if there's no jQuery, there is no events
            }
        };

        //wire up the navigate events, wrap method to maintain scope
        $(window).bind('hashchange', function(e) { obj.onNavigate.call(obj); });

        return obj;
    };

})(jQuery);