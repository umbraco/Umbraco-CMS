/**
 * @ngdoc filter
 * @name umbraco.filters.filter:umbCmsJoinArray
 * @namespace umbCmsJoinArray
 * 
 * param {array} array of string or objects, if an object use the third argument to specify which prop to list.
 * param {seperator} string containing the seperator to add between joined values.
 * param {prop} string used if joining an array of objects, set the name of properties to join.
 * 
 * @description
 * Join an array of string or an array of objects, with a costum seperator.
 * 
 */
angular.module("umbraco.filters").filter('umbCmsJoinArray', function () {
    return function join(array, separator, prop) {
        return (!angular.isUndefined(prop) ? array.map(function (item) {
            return item[prop];
        }) : array).join(separator || '');
    };
});
