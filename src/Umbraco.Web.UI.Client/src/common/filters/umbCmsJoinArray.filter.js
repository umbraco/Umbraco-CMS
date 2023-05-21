/**
 * @ngdoc filter
 * @name umbraco.filters.filter:umbCmsJoinArray
 * @namespace umbCmsJoinArray
 * 
 * param {array} array of string or objects, if an object use the third argument to specify which prop to list.
 * param {separator} string containing the separator to add between joined values.
 * param {prop} string used if joining an array of objects, set the name of properties to join.
 * 
 * @description
 * Join an array of string or an array of objects, with a custom separator.
 * If the array is null or empty, returns an empty string
 * If the array is not actually an array (ie a string or number), returns the value of the array
 */
angular.module("umbraco.filters").filter('umbCmsJoinArray', function () {
    return function join(array, separator, prop) {
        if (typeof array !== 'object' || !array) {
            return array || '';
        }
        separator = separator || '';
        return (!Utilities.isUndefined(prop) ? array.map(item => item[prop]) : array).join(separator);
    };
});
