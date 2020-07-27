
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
                ctrl.$validators["pattern"] = validatePattern;
            }

            function validatePattern(modelValue, viewValue) {
                var value = modelValue || viewValue;
                return /^[0-9]*$/.test(value);
            }            
        }
    };
}
angular.module('umbraco.directives').directive("onlyDigits", onlyDigits);
