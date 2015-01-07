/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valRegex
    * @restrict A
    * @description A custom directive to allow for matching a value against a regex string.
    *               NOTE: there's already an ng-pattern but this requires that a regex expression is set, not a regex string
    **/
function valRegex() {
    
    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, elm, attrs, ctrl) {

            var flags = "";
            if (attrs.valRegexFlags) {
                try {
                    flags = scope.$eval(attrs.valRegexFlags);
                    if (!flags) {
                        flags = attrs.valRegexFlags;
                    }
                }
                catch (e) {
                    flags = attrs.valRegexFlags;
                }
            }
            var regex;
            try {
                var resolved = scope.$eval(attrs.valRegex);                
                if (resolved) {
                    regex = new RegExp(resolved, flags);
                }
                else {
                    regex = new RegExp(attrs.valRegex, flags);
                }
            }
            catch(e) {
                regex = new RegExp(attrs.valRegex, flags);
            }

            var patternValidator = function (viewValue) {
                //NOTE: we don't validate on empty values, use required validator for that
                if (!viewValue || regex.test(viewValue)) {
                    // it is valid
                    ctrl.$setValidity('valRegex', true);
                    //assign a message to the validator
                    ctrl.errorMsg = "";
                    return viewValue;
                }
                else {
                    // it is invalid, return undefined (no model update)
                    ctrl.$setValidity('valRegex', false);
                    //assign a message to the validator
                    ctrl.errorMsg = "Value is invalid, it does not match the correct pattern";
                    return undefined;
                }
            };

            ctrl.$formatters.push(patternValidator);
            ctrl.$parsers.push(patternValidator);
        }
    };
}
angular.module('umbraco.directives.validation').directive("valRegex", valRegex);