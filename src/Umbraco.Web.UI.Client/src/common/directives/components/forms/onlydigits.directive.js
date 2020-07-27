
/**
* @ngdoc directive
* @name umbraco.directives.directive:onlyDigits
* @restrict A
* @description Used in conjunction with type='number' input fields to check if a field should accept digits only. 
**/
function onlyDigits($parse) {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, elem, attrs, ctrl) {

            var allowOnlyDigits = scope.$eval(attrs.onlyDigits);

            if (allowOnlyDigits) {
                ctrl.$parsers.push(function (input) {
                    if (input === undefined || input === null) return '';
                    var inputAsStr = input.toString();
                    var transformedInput = inputAsStr.replace(/\D/g, '');
                    if (transformedInput !== inputAsStr) {
                        ctrl.$setViewValue(transformedInput);
                        ctrl.$render();
                    }
                    return transformedInput;
                });
            }
        }
    };
}
angular.module('umbraco.directives').directive("onlyDigits", onlyDigits);
