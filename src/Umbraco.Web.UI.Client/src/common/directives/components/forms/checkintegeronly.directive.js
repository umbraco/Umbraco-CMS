
/**
* @ngdoc directive
* @name umbraco.directives.directive:checkIntegerOnly
* @restrict A
* @description Used in conjunction with type='number' input fields to check if a field should accept integers only. 
*              This condition is optional based on the property editor configuration specifying that the number is expected to be an integer.
**/
function checkIntegerOnly($parse) {
    return {
        restrict: "A",
        require: "ngModel",

        link: function (scope, elem, attrs, ctrl) {
            if (scope.$parent.model.config && scope.$parent.model.config.integer) {
                elem.on('keydown', function (event) {
                    if ([110, 190].indexOf(event.which) > -1) { // Check for decimal point
                        event.preventDefault();
                        return false;
                    }

                    return true;
                });
            }
        }
    };
}
angular.module('umbraco.directives').directive("checkIntegerOnly", checkIntegerOnly);
