/**
 * @ngdoc filter
 * @name umbraco.filters.filter:truncate
 * @namespace truncateFilter
 * 
 * param {any} wordwise if true, the string will be cut after last fully displayed word.
 * param {any} max where to cut the string.
 * param {any} tail option tail, defaults to: '…'
 * 
 * Legacy version:
 * parameter noOfChars(wordwise) Where to cut the string.
 * parameter appendDots(max) If true dots will be appended in the end.
 * 
 * @description
 * Limits the length of a string, if a cut happens only the string will be appended with three dots to indicate that more is available.
 */
angular.module("umbraco.filters").filter('truncate', 
    function () {
        return function (value, wordwise, max, tail) {
            
            if (!value) return '';
            
            // Overload-fix to support Forms Legacy Version:
            if (typeof(wordwise) !== 'boolean') {
                // switch arguments around to fit Forms version.
                if (max !== true) {
                    tail = '';
                }
                max = wordwise;
                wordwise = false;
            }
            // !end of overload fix.
            
            max = parseInt(max, 10);
            if (!max) return value;
            if (value.length <= max) return value;
            
            tail = (!tail && tail !== '') ? '…' : tail;
            
            if (wordwise && value.substr(max, 1) === ' ') {
                max++;
            }
            value = value.substr(0, max);
            
            if (wordwise) {
                var lastspace = value.lastIndexOf(' ');
                if (lastspace !== -1) {
                    value = value.substr(0, lastspace+1);
                }
            }
            
            return value + tail;
        };
    }
);
