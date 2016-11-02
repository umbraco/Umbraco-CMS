/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valEmail
    * @restrict A
    * @description A custom directive to validate an email address string, this is required because angular's default validator is incorrect.
    **/
function valEmail(valEmailExpression) {
   
    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, elm, attrs, ctrl) {
            
            var patternValidator = function (viewValue) {
                //NOTE: we don't validate on empty values, use required validator for that
                if (!viewValue || valEmailExpression.EMAIL_REGEXP.test(viewValue)) {
                    // it is valid
                    ctrl.$setValidity('valEmail', true);
                    //assign a message to the validator
                    ctrl.errorMsg = "";
                    return viewValue;
                }
                else {
                    // it is invalid, return undefined (no model update)
                    ctrl.$setValidity('valEmail', false);
                    //assign a message to the validator
                    ctrl.errorMsg = "Invalid email";
                    return undefined;
                }
            };

            //if there is an attribute: type="email" then we need to remove those formatters and parsers
            if (attrs.type === "email") {
                //we need to remove the existing parsers = the default angular one which is created by
                // type="email", but this has a regex issue, so we'll remove that and add our custom one
                ctrl.$parsers.pop();
                //we also need to remove the existing formatter - the default angular one will not render
                // what it thinks is an invalid email address, so it will just be blank
                ctrl.$formatters.pop();
            }
            
            ctrl.$parsers.push(patternValidator);
        }
    };
}

angular.module('umbraco.directives.validation')
    .directive("valEmail", valEmail)
    .factory('valEmailExpression', function () {
        //NOTE: This is the fixed regex which is part of the newer angular
        return {
            EMAIL_REGEXP: /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i
        };
    });