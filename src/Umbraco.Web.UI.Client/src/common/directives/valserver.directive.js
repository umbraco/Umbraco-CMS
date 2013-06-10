/**
    * @ngdoc directive 
    * @name umbraco.directive:valServer
    * @restrict A
    * @description This directive is used to associate a field with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServer() {
    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, element, attr, ctrl) {
            if (!scope.model || !scope.model.alias){
                throw "valServer can only be used in the scope of a content property object";
            }
            var parentErrors = scope.$parent.serverErrors;
            if (!parentErrors) {
                return;
            }

            var fieldName = scope.$eval(attr.valServer);

            //subscribe to the changed event of the element. This is required because when we
            // have a server error we actually invalidate the form which means it cannot be 
            // resubmitted. So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute.
            element.keydown(function () {
                if (ctrl.$invalid) {
                    ctrl.$setValidity('valServer', true);
                }
            });
            element.change(function () {
                if (ctrl.$invalid) {
                    ctrl.$setValidity('valServer', true);
                }
            });
            //TODO: DO we need to watch for other changes on the element ?

            //subscribe to the server validation changes
            parentErrors.subscribe(scope.model, fieldName, function (isValid, propertyErrors, allErrors) {
                if (!isValid) {
                    ctrl.$setValidity('valServer', false);
                    //assign an error msg property to the current validator
                    ctrl.errorMsg = propertyErrors[0].errorMsg;
                }
                else {
                    ctrl.$setValidity('valServer', true);
                    //reset the error message
                    ctrl.errorMsg = "";
                }
            }, true);
        }
    };
}
angular.module('umbraco.directives').directive("valServer", valServer);