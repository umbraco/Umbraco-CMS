/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServerField
    * @restrict A
    * @description This directive is used to associate a content field (not user defined) with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServerField(serverValidationManager) {
    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, element, attr, ctrl) {
            
            if (!attr.valServerField) {
                throw "valServerField must have a field name for referencing server errors";
            }

            var fieldName = attr.valServerField;
            
            //subscribe to the changed event of the view model. This is required because when we
            // have a server error we actually invalidate the form which means it cannot be 
            // resubmitted. So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute.
            ctrl.$viewChangeListeners.push(function () {
                if (ctrl.$invalid) {
                    ctrl.$setValidity('valServerField', true);
                }
            });
            
            //subscribe to the server validation changes
            serverValidationManager.subscribe(null, fieldName, function (isValid, fieldErrors, allErrors) {
                if (!isValid) {
                    ctrl.$setValidity('valServerField', false);
                    //assign an error msg property to the current validator
                    ctrl.errorMsg = fieldErrors[0].errorMsg;
                }
                else {
                    ctrl.$setValidity('valServerField', true);
                    //reset the error message
                    ctrl.errorMsg = "";
                }
            });
            
            //when the element is disposed we need to unsubscribe!
            // NOTE: this is very important otherwise when this controller re-binds the previous subscriptsion will remain
            // but they are a different callback instance than the above.
            element.bind('$destroy', function () {
                serverValidationManager.unsubscribe(null, fieldName);
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valServerField", valServerField);