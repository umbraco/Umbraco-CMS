angular.module("umbraco.filters")
    .filter('splitOnUpperCase', function () {
        return function(stringToSplit) {

            if (isNaN(stringToSplit)) {
                return stringToSplit.match(/[A-Z][a-z]+|[0-9]+/g).join(" ");
            } else {
                return stringToSplit;
            }
        };
    });