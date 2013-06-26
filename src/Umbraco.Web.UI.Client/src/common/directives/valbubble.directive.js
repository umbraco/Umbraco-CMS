/**
* @ngdoc directive 
* @name umbraco.directive:valBubble
* @restrict A
* @description This directive will bubble up a notification via an emit event (upwards)
                describing the state of the validation element. This is useful for 
                parent elements to know about child element validation state.
**/
function valBubble() {
    return {
        require: ['ngModel', '^form'],
        link: function (scope, element, attr, ctrls) {

            if (!attr.name) {
                throw "valBubble must be set on an input element that has a 'name' attribute";
            }

            var modelCtrl = ctrls[0];
            var formCtrl = ctrls[1];

            //watch the current form's validation for the current field name
            scope.$watch(formCtrl.$name + "." + modelCtrl.$name + ".$valid", function (isValid, lastValue) {
                if (isValid !== undefined) {
                    //emit an event upwards 
                    scope.$emit("valBubble", {
                        isValid: isValid,       // if the field is valid
                        element: element,       // the element that the validation applies to
                        expression: this.exp,   // the expression that was watched to check validity
                        scope: scope,           // the current scope
                        formCtrl: formCtrl   // the current form controller
                    });
                }
            });
        }
    };
}
angular.module('umbraco.directives').directive("valBubble", valBubble);