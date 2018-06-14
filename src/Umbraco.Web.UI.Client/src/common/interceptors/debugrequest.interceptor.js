(function() {
   'use strict';

    /**
     * Used to set debug headers on all requests where necessary
     * @param {any} $q
     * @param {any} urlHelper
     */
    function debugRequestInterceptor($q, urlHelper) {
        return {
            //dealing with requests:
            'request': function(config) {
                var queryStrings = urlHelper.getQueryStringParams();
                if (queryStrings.umbDebug === "true" || queryStrings.umbdebug === "true") {
                    config.headers["X-UMB-DEBUG"] = "true";
                }
                return config;
            }
        };
   }

    angular.module('umbraco.interceptors').factory('debugRequestInterceptor', debugRequestInterceptor);


})();
