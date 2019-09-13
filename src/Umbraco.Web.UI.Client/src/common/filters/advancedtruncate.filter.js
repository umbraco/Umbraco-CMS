/**
 * @ngdoc filter
 * @name umbraco.filters.filter:advancedTruncate
 * @namespace advancedTruncateFilter
 * 
 * param {any} wordwise if true, the string will be cut after last fully displayed word.
 * param {any} max max length of the outputtet string
 * param {any} tail option tail, defaults to: ' ...'
 *
 * @description
 * Limits the length of a string, if a cut happens only the string will be appended with three dots to indicate that more is available.
 */
angular.module("umbraco.filters").filter('advancedTruncate', 
    function () {
        return function (value, wordwise, max, tail) {
            if (!value) return '';
            max = parseInt(max, 10);
            if (!max) return value;
            if (value.length <= max) return value;

            value = value.substr(0, max);
            if (wordwise) {
                var lastspace = value.lastIndexOf(' ');
                if (lastspace != -1) {
                    value = value.substr(0, lastspace);
                }
            }
            return value + (tail || (wordwise ? ' …' : '…'));
        };
    }
);
