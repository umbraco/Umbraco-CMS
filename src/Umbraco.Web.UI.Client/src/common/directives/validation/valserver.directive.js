/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServer
    * @restrict A
    * @description This directive is used to associate a content property with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServer(serverValidationManager) {
    return {
        require: ['ngModel', '?^umbProperty'],
        restrict: "A",
        link: function (scope, element, attr, ctrls) {

            var modelCtrl = ctrls[0];
            var umbPropCtrl = ctrls.length > 1 ? ctrls[1] : null;
            if (!umbPropCtrl) {
                //we cannot proceed, this validator will be disabled
                return;
            }

            var watcher = null;

            //Need to watch the value model for it to change, previously we had  subscribed to 
            //modelCtrl.$viewChangeListeners but this is not good enough if you have an editor that
            // doesn't specifically have a 2 way ng binding. This is required because when we
            // have a server error we actually invalidate the form which means it cannot be 
            // resubmitted. So once a field is changed that has a server error assigned to it
            // we need to re-validate it for the server side validator so the user can resubmit
            // the form. Of course normal client-side validators will continue to execute.
            function startWatch() {
                //if there's not already a watch
                if (!watcher) {
                    watcher = scope.$watch(function () {
                        return modelCtrl.$modelValue;
                    }, function (newValue, oldValue) {

                        if (!newValue || angular.equals(newValue, oldValue)) {
                            return;
                        }

                        if (modelCtrl.$invalid) {
                            modelCtrl.$setValidity('valServer', true);
                            stopWatch();
                        }
                    }, true);
                }
            }

            function stopWatch() {
                if (watcher) {
                    watcher();
                    watcher = null;
                }
            }

            var currentProperty = umbPropCtrl.property;

            //default to 'value' if nothing is set
            var fieldName = "value";
            if (attr.valServer) {
                fieldName = scope.$eval(attr.valServer);
                if (!fieldName) {
                    //eval returned nothing so just use the string
                    fieldName = attr.valServer;
                }
            }            
            
            //subscribe to the server validation changes
            serverValidationManager.subscribe(currentProperty.alias, fieldName, function (isValid, propertyErrors, allErrors) {
                if (!isValid) {
                    modelCtrl.$setValidity('valServer', false);
                    //assign an error msg property to the current validator
                    modelCtrl.errorMsg = propertyErrors[0].errorMsg;
                    startWatch();
                }
                else {
                    modelCtrl.$setValidity('valServer', true);
                    //reset the error message
                    modelCtrl.errorMsg = "";
                    stopWatch();
                }
            });
            
            //when the element is disposed we need to unsubscribe!
            // NOTE: this is very important otherwise when this controller re-binds the previous subscriptsion will remain
            // but they are a different callback instance than the above.
            element.bind('$destroy', function () {
                stopWatch();
                serverValidationManager.unsubscribe(currentProperty.alias, fieldName);
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valServer", valServer);