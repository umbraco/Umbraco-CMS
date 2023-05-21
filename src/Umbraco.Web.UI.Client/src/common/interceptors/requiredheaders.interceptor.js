(function() {
   'use strict';

    /**
     * Used to set required headers on all requests where necessary
     * @param {any} $q
     * @param {any} urlHelper
     */
    function requiredHeadersInterceptor($q, urlHelper) {
        return {
            //dealing with requests:
            'request': function (config) {

                // This is a standard header that should be sent for all ajax requests and is required for 
                // how the server handles auth rejections, etc... see https://github.com/dotnet/aspnetcore/blob/a2568cbe1e8dd92d8a7976469100e564362f778e/src/Security/Authentication/Cookies/src/CookieAuthenticationEvents.cs#L106-L107
                config.headers["X-Requested-With"] = "XMLHttpRequest";

                // Set the debug header if in debug mode
                var queryStrings = urlHelper.getQueryStringParams();
                if (queryStrings.umbDebug === "true" || queryStrings.umbdebug === "true") {
                    config.headers["X-UMB-DEBUG"] = "true";
                }
                return config;
            }
        };
   }

    angular.module('umbraco.interceptors').factory('requiredHeadersInterceptor', requiredHeadersInterceptor);


})();
