/**
 * @ngdoc filter
 * @name umbraco.filters.filter:umbCmsTitleCase
 * @namespace umbCmsTitleCase
 * 
 * param {string} the text turned into title case.
 * 
 * @description
 * Transforms text to title case. Capitalizes the first letter of each word, and transforms the rest of the word to lower case.
 * 
 */
angular.module("umbraco.filters").filter('umbCmsTitleCase', function() {
    return function (str) {
        return str.replace(
            /\w\S*/g,
            txt => txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
        );
    }
});
