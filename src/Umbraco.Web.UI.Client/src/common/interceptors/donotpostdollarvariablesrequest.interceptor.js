(function () {
    'use strict';

    function removeProperty(obj, propertyPrefix) {
        for (var property in obj) {
            if (obj.hasOwnProperty(property)) {

                if (property.startsWith(propertyPrefix) && obj[property] !== undefined) {
                    obj[property] = undefined;
                }

                if (typeof obj[property] === "object") {
                    removeProperty(obj[property], propertyPrefix);
                }
            }
        }

    }

    function transform(data) {
        removeProperty(data, "$");
    }

    function doNotPostDollarVariablesRequestInterceptor($q, urlHelper) {
        return {
            //dealing with requests:
            'request': function (config) {
                if (config.method === "POST") {
                    var clone = Utilities.copy(config);
                    transform(clone.data);
                    return clone;
                }

                return config;
            }
        };
    }

    angular.module('umbraco.interceptors').factory('doNotPostDollarVariablesOnPostRequestInterceptor', doNotPostDollarVariablesRequestInterceptor);
})();
