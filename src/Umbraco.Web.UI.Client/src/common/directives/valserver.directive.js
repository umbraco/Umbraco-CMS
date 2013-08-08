/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServer
    * @restrict A
    * @description This directive is used to associate a content property with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServer(serverValidationManager) {
    return {
        require: 'ngModel',
        restrict: "A",
        link: function (scope, element, attr, ctrl) {
            
            if (!scope.alias){
                throw "valServer can only be used in the scope of a content property object";
            }
            
            var fieldName = scope.$eval(attr.valServer);
            if (!fieldName) {
                //eval returned nothing so just use the string
                fieldName = attr.valServer;
            }

            //subscribe to the changed event of the view model. This is required because when we
            // have a server error we actually invalidate the form which means it cannot be 
            // resubmitted. So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute.
            ctrl.$viewChangeListeners.push(function () {
                if (ctrl.$invalid) {
                    ctrl.$setValidity('valServer', true);
                    //emit the event upwards
                    scope.$emit("serverRevalidated", { modelCtrl: ctrl });
                }
            });
            
            //subscribe to the server validation changes
            serverValidationManager.subscribe(scope.alias, fieldName, function (isValid, propertyErrors, allErrors) {
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
            });
            
            //when the element is disposed we need to unsubscribe!
            // NOTE: this is very important otherwise when this controller re-binds the previous subscriptsion will remain
            // but they are a different callback instance than the above.
            element.bind('$destroy', function () {
                serverValidationManager.unsubscribe(scope.alias, fieldName);
            });
        }
    };
}
angular.module('umbraco.directives').directive("valServer", valServer);