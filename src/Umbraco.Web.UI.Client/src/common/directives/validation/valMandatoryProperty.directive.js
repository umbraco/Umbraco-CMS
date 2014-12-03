/**
* @ngdoc directive
* @name umbraco.directives.directive:valMandatoryProperty
* @restrict A
* @description Performs the mandatory property editors validation.
*              The mandatory validation is different depending on the property editor (for instance for the TAGS property editor it means to have at least one tag selected).
               In order to avoid to handle here all types of mandatory validations, this directive will call a method called 'validateMandatoryProperty()' which
               will return true/false indicating a valid/invalid result.
**/

function valMandatoryProperty(serverValidationManager) {
    return {

        // The element must have ng-model attribute.
        require: 'ngModel',

        // Restrict the directive to an attribute type.
        restrict: "A",

        // scope = the parent scope
        // element = the element the directive is on
        // attrs = a dictionary of attributes on the element
        // ctrl = the controller for ngModel.
        link: function (scope, element, attrs, ctrl) {

            // Check whether the scope is not null and the property is marked as mandatory
            if (!scope || !scope.model.validation.mandatory) {
                return;
            }

            // Check whether the scope has a validateMandatoryProperty() method 
            if (!scope.validateMandatoryProperty) {
                throw new Error('val-Mandatory-Property directive requires that the scope has a validateMandatoryProperty() method.');
            }

            // Validation method
            var validate = function (viewValue) {
                // Calls the validition method defined in the scope
                if (scope.validateMandatoryProperty()) {
                    ctrl.$setValidity('required', true); // Tell the controlller that the value is valid
                    ctrl.errorMsg = "";
                    return viewValue;
                }
                else {
                    ctrl.$setValidity('required', false); // Tell the controlller that the value is invalid
                    ctrl.errorMsg = "This property is mandatory";
                    return viewValue;
                }
            };

            // Formatters are invoked when the model is modified in the code.
            ctrl.$formatters.push(validate);

            // Parsers are called as soon as the value in the form input is modified
            ctrl.$parsers.push(validate);

            // Listen for the forms saving event to validate 
            scope.$on("formSubmitting", function (ev, args) {
                validate(element.val());
            });

        }
    };
}
angular.module('umbraco.directives.validation').directive("valMandatoryProperty", valMandatoryProperty);