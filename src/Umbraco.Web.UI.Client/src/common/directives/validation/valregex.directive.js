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
            var regex;
            var eventBindings = [];

            attrs.$observe("valRegexFlags", function (newVal) {
                if (newVal) {
                    flags = newVal;
                }
            });

            attrs.$observe("valRegex", function (newVal) {
                if (newVal) {
                    try {
                        var resolved = newVal;
                        if (resolved) {
                            regex = new RegExp(resolved, flags);
                        }
                        else {
                            regex = new RegExp(attrs.valRegex, flags);
                        }
                    }
                    catch (e) {
                        regex = new RegExp(attrs.valRegex, flags);
                    }
                }
            });

            eventBindings.push(scope.$watch('ngModel', function(newValue, oldValue){
                if(newValue && newValue !== oldValue) {
                    patternValidator(newValue);
                }
            }));

            var patternValidator = function (viewValue) {
                if (regex) {
                    //NOTE: we don't validate on empty values, use required validator for that
                if (!viewValue || regex.test(viewValue.toString())) {
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
                }
            };

            scope.$on('$destroy', function(){
              // unbind watchers
              for(var e in eventBindings) {
                eventBindings[e]();
               }
            });

        }
    };
}
angular.module('umbraco.directives.validation').directive("valRegex", valRegex);
