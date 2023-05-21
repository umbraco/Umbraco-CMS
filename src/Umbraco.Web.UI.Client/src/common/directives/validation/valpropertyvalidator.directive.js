/**
* @ngdoc directive
* @name umbraco.directives.directive:valPropertyValidator
* @restrict A
* @description Performs any custom property value validation checks on the client side. This allows property editors to be highly flexible when it comes to validation
                on the client side. Typically if a property editor stores a primitive value (i.e. string) then the client side validation can easily be taken care of 
                with standard angular directives such as ng-required. However since some property editors store complex data such as JSON, a given property editor
                might require custom validation. This directive can be used to validate an Umbraco property in any way that a developer would like by specifying a
                callback method to perform the validation. The result of this method must return an object in the format of 
                {isValid: true, errorKey: 'required', errorMsg: 'Something went wrong' }
                The error message returned will also be displayed for the property level validation message.
                This directive should only be used when dealing with complex models, if custom validation needs to be performed with primitive values, use the simpler 
                angular validation directives instead since this will watch the entire model. 
**/

function valPropertyValidator(serverValidationManager) {
    return {
        scope: {
            valPropertyValidator: "="
        },

        // The element must have ng-model attribute and be inside an umbProperty directive
        require: ['ngModel', '?^umbProperty'],

        restrict: "A",

        link: function (scope, element, attrs, ctrls) {

            var modelCtrl = ctrls[0];
            var propCtrl = ctrls.length > 1 ? ctrls[1] : null;

            // Check whether the scope has a valPropertyValidator method
            if (!scope.valPropertyValidator || !Utilities.isFunction(scope.valPropertyValidator)) {
                throw new Error('val-property-validator directive must specify a function to call');
            }
            
            // Validation method
            function validate (viewValue) {
                // Calls the validation method
                var result = scope.valPropertyValidator();
                if (!result.errorKey || result.isValid === undefined || !result.errorMsg) {
                    throw "The result object from valPropertyValidator does not contain required properties: isValid, errorKey, errorMsg";
                }
                if (result.isValid === true) {
                    // Tell the controller that the value is valid
                    modelCtrl.$setValidity(result.errorKey, true);
                    if (propCtrl) {
                        propCtrl.setPropertyError(null);
                    }                    
                }
                else {
                    // Tell the controller that the value is invalid
                    modelCtrl.$setValidity(result.errorKey, false);
                    if (propCtrl) {
                        propCtrl.setPropertyError(result.errorMsg);
                    }
                }

                // parsers are expected to return a value
                return (result.isValid) ? viewValue : undefined;
            };

            // Parsers are called as soon as the value in the form input is modified
            modelCtrl.$parsers.push(validate);

            //call on init
            validate();

        }
    };
}
angular.module('umbraco.directives.validation').directive("valPropertyValidator", valPropertyValidator);
