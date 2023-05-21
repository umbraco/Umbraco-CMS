/**
 * @ngdoc filter
 * @name umbraco.filters.filter:umbWordLimit
 * @namespace umbWordLimitFilter
 *
 * @description
 * Limits the number of words in a string to the passed in value
 */

(function () {
    'use strict';

    function umbWordLimitFilter() {
        return function (collection, property) {

            if (!Utilities.isString(collection)) {
                return collection;
            }

            if (Utilities.isUndefined(property)) {
                return collection;
            }

            var newString = "";
            var array = [];

            array = collection.split(" ", property);
            array.length = property;
            newString = array.join(" ");
            
            return newString;

        };
    }

    angular.module('umbraco.filters').filter('umbWordLimit', umbWordLimitFilter);

})();
