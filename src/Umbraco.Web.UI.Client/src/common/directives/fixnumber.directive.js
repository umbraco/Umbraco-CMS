
/**
* @ngdoc directive
* @name umbraco.directives.directive:fixNumber
* @restrict A
* @description Used in conjunction with type='number' input fields to ensure that the bound value is converted to a number when using ng-model
*  because normally it thinks it's a string and also validation doesn't work correctly due to an angular bug.
**/
function fixNumber() {
    return {
        restrict: "A",
        require: "ngModel",
        link: function (scope, element, attr, ngModel) {

            //This fixes the issue of when your model contains a number as a string (i.e. "1" instead of 1)
            // which will not actually work on initial load and the browser will say you have an invalid number 
            // entered. So if it parses to a number, we call setViewValue which sets the bound model value
            // to the real number. It should in theory update the view but it doesn't so we need to manually set
            // the element's value. I'm sure there's a bug logged for this somewhere for angular too.

            var modelVal = scope.$eval(attr.ngModel);
            if (modelVal) {
                var asNum = parseFloat(modelVal, 10);
                if (!isNaN(asNum)) {                    
                    ngModel.$setViewValue(asNum);
                    element.val(asNum);
                }
                else {                    
                    ngModel.$setViewValue(null);
                    element.val("");
                }
            }
            
            ngModel.$formatters.push(function (value) {
                if (angular.isString(value)) {
                    return parseFloat(value);
                }
                return value;
            });
            
            //This fixes this angular issue: 
            //https://github.com/angular/angular.js/issues/2144
            // which doesn't actually validate the number input properly since the model only changes when a real number is entered
            // but the input box still allows non-numbers to be entered which do not validate (only via html5)
            
            if (typeof element.prop('validity') === 'undefined') {
                return;
            }

            element.bind('input', function (e) {
                var validity = element.prop('validity');
                scope.$apply(function () {
                    ngModel.$setValidity('number', !validity.badInput);
                });
            });

        }
    };
}
angular.module('umbraco.directives').directive("fixNumber", fixNumber);