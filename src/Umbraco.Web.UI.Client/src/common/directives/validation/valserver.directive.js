/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServer
    * @restrict A
    * @description This directive is used to associate a content property with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServer(serverValidationManager) {
    return {
        require: ['ngModel', '?^^umbProperty', '?^^umbVariantContent'],
        restrict: "A",
        scope: {},
        link: function (scope, element, attr, ctrls) {

            var modelCtrl = ctrls[0];
            var umbPropCtrl = ctrls[1];
            if (!umbPropCtrl) {
                //we cannot proceed, this validator will be disabled
                return;
            }
            
            // optional reference to the varaint-content-controller, needed to avoid validation when the field is invariant on non-default languages.
            var umbVariantCtrl = ctrls[2];

            var currentProperty = umbPropCtrl.property;
            var currentCulture = currentProperty.culture;
            var currentSegment = currentProperty.segment;

            if (umbVariantCtrl) {
                //if we are inside of an umbVariantContent directive

                var currentVariant = umbVariantCtrl.editor.content;

                // Lets check if we have variants and we are on the default language then ...
                if (umbVariantCtrl.content.variants.length > 1 && (!currentVariant.language || !currentVariant.language.isDefault) && !currentCulture && !currentSegment && !currentProperty.unlockInvariantValue) {
                    //This property is locked cause its a invariant property shown on a non-default language.
                    //Therefor do not validate this field.
                    return;
                }
            }
                       
            // if we have reached this part, and there is no culture, then lets fallback to invariant. To get the validation feedback for invariant language.
            currentCulture = currentCulture || "invariant";
            
            var watcher = null;
            var unsubscribe = [];

            //default to 'value' if nothing is set
            var fieldName = "value";
            if (attr.valServer) {
                fieldName = scope.$eval(attr.valServer);
                if (!fieldName) {
                    //eval returned nothing so just use the string
                    fieldName = attr.valServer;
                }
            }

            // Get the property validation path if there is one, this is how wiring up any nested/virtual property validation works
            var propertyValidationPath = umbPropCtrl ? umbPropCtrl.getValidationPath() : currentProperty.alias;

            // Need to watch the value model for it to change, previously we had  subscribed to 
            // modelCtrl.$viewChangeListeners but this is not good enough if you have an editor that
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

                        if (!newValue || Utilities.equals(newValue, oldValue)) {
                            return;
                        }

                        if (modelCtrl.$invalid) {
                            modelCtrl.$setValidity('valServer', true);
                            
                            //clear the server validation entry
                            serverValidationManager.removePropertyError(propertyValidationPath, currentCulture, fieldName, currentSegment);

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
            
            //subscribe to the server validation changes
            function serverValidationManagerCallback(isValid, propertyErrors, allErrors) {
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
            }

            unsubscribe.push(serverValidationManager.subscribe(
                propertyValidationPath,
                currentCulture,
                fieldName,
                serverValidationManagerCallback,
                currentSegment)
            );

            scope.$on('$destroy', function () {
                stopWatch();
                unsubscribe.forEach(u => u());
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valServer", valServer);
