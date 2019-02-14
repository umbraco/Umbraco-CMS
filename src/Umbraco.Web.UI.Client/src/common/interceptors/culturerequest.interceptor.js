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
            'request': function (config) {
                if (!Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath) {
                    // no settings available, we're probably on the login screen
                    return config;
                }
                if (!config.url.match(RegExp(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "\/backoffice\/", "i"))) {
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
