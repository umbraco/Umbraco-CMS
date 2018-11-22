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

            function patternValidator(viewValue) {
                //NOTE: we don't validate on empty values, use required validator for that
                if (!viewValue || valEmailExpression.EMAIL_REGEXP.test(viewValue)) {
                    //assign a message to the validator
                    ctrl.errorMsg = "";
                    return true;
                }
                else {
                    //assign a message to the validator
                    ctrl.errorMsg = "Invalid email";
                    return false;
                }
            };

            //if there is an attribute: type="email" then we need to remove the built in validator
            // this isn't totally required but it gives us the ability to completely control the validation syntax so we don't
            // run into old problems like http://issues.umbraco.org/issue/U4-8445
            if (attrs.type === "email") {
                ctrl.$validators = {};
            }

            ctrl.$validators.valEmail = function(modelValue, viewValue) {
                return patternValidator(viewValue);
            };

        }
    };
}

angular.module('umbraco.directives.validation')
    .directive("valEmail", valEmail)
    .factory('valEmailExpression', function () {
        var emailRegex = /^[a-z0-9!#$%&'*+\/=?^_`{|}~.-]+@[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i;
        return {
            EMAIL_REGEXP: emailRegex
        };
    });
