
/**
* @ngdoc directive
* @name umbraco.directives.directive:fixNumber
* @restrict A
* @description Used in conjunction with type='number' input fields to ensure that the bound value is converted to a number when using ng-model
*  because normally it thinks it's a string and also validation doesn't work correctly due to an angular bug.
**/
function fixNumber($parse) {
    return {
        restrict: "A",
        require: "ngModel",

        link: function (scope, elem, attrs, ctrl) {

            //parse ngModel onload
            var modelVal = scope.$eval(attrs.ngModel);
            if (modelVal) {
                var asNum = parseFloat(modelVal, 10);
                if (!isNaN(asNum)) {
                    $parse(attrs.ngModel).assign(scope, asNum);
                }
            }

            //always return an int to the model
            ctrl.$parsers.push(function (value) {
                if (value === 0) {
                    return 0;
                }
                return parseFloat(value || '', 10);
            });

            //always try to format the model value as an int
            ctrl.$formatters.push(function (value) {
                if (Utilities.isString(value)) {
                    return parseFloat(value, 10);
                }
                return value;
            });

            //This fixes this angular issue: 
            //https://github.com/angular/angular.js/issues/2144
            // which doesn't actually validate the number input properly since the model only changes when a real number is entered
            // but the input box still allows non-numbers to be entered which do not validate (only via html5)
            if (typeof elem.prop('validity') === 'undefined') {
                return;
            }

            elem.on('input', function (e) {
                var validity = elem.prop('validity');
                scope.$apply(function () {
                    ctrl.$setValidity('number', !validity.badInput);
                });
            });
        }
    };
}
angular.module('umbraco.directives').directive("fixNumber", fixNumber);
