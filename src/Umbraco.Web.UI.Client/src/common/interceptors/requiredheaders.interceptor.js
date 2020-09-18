(function () {
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
                // how the server handles auth rejections, etc... see
                // https://github.com/aspnet/AspNetKatana/blob/e2b18ec84ceab7ffa29d80d89429c9988ab40144/src/Microsoft.Owin.Security.Cookies/Provider/DefaultBehavior.cs
                // https://brockallen.com/2013/10/27/using-cookie-authentication-middleware-with-web-api-and-401-response-codes/
                config.headers["X-Requested-With"] = "XMLHttpRequest";

                return config;
            }
        };
    }

    angular.module('umbraco.interceptors').factory('requiredHeadersInterceptor', requiredHeadersInterceptor);


})();
