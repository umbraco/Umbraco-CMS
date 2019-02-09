(function() {
   'use strict';

    /**
     * Used to set the current client culture on all requests API requests
     * @param {any} $q
     * @param {any} $routeParams
     */
    function cultureRequestInterceptor($q, $routeParams) {
        return {
            //dealing with requests:
            'request': function(config) {
                var apiPattern = /\/umbraco\/backoffice\//;
                if (!apiPattern.test(config.url)) {
                    // it's not an API request, no handling
                    return config;
                }
                // it's an API request, add the current client culture as a header value
                config.headers["X-UMB-CULTURE"] = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
                return config;
            }
        };
   }

    angular.module('umbraco.interceptors').factory('cultureRequestInterceptor', cultureRequestInterceptor);

})();
