/**
    * @ngdoc directive
    * @name umbraco.directives.directive:valServer
    * @restrict A
    * @description This directive is used to associate a content property with a server-side validation response
    *               so that the validators in angular are updated based on server-side feedback.
    **/
function valServer(serverValidationManager) {
    return {
        require: ['ngModel', '?^^umbProperty', '?^^umbVariantContent', '?^^umbNestedProperty'],
        restrict: "A",
        scope: {},
        link: function (scope, element, attr, ctrls) {

            var modelCtrl = ctrls[0];
            var umbPropCtrl = ctrls.length > 1 ? ctrls[1] : null;
            if (!umbPropCtrl) {
                //we cannot proceed, this validator will be disabled
                return;
            }
            
            // optional reference to the varaint-content-controller, needed to avoid validation when the field is invariant on non-default languages.
            var umbVariantCtrl = ctrls.length > 2 ? ctrls[2] : null;
            var umbNestedPropertyCtrl = ctrls.length > 3 ? ctrls[3] : null;

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

                            //clear the server validation entry
                            // TODO: We'll need to handle this differently since this will need to target the actual 'fieldName' or validation
                            // path if there is one
                            serverValidationManager.removePropertyError(currentProperty.alias, currentCulture, fieldName, currentSegment);
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

            // TODO: If this is a property/field within a complex editor which means it could be a nested/nested/nested property/field           
            // TODO: We have a block $id to work with now so that is what we should be looking to use for the 'key'

            var propertyValidationPath = umbNestedPropertyCtrl ? umbNestedPropertyCtrl.getValidationPath() : null;

            unsubscribe.push(serverValidationManager.subscribe(
                currentProperty.alias,
                currentCulture,
                // use the propertyValidationPath for the fieldName value if there is one since if there is one it means it's a complex
                // editor and as such the 'fieldName' will be empty. The serverValidationManager knows how to handle the jsonpath
                // string as the fieldName.
                // TODO: This isn't quite true! If there is a fieldName specified, then it will need to be added to the 
                // validation path. We should pass in the fieldName to umbNestedPropertyCtrl.getValidationPath(); since this could very well be targeting a specific field

                propertyValidationPath ? propertyValidationPath : fieldName,
                serverValidationManagerCallback,
                currentSegment)
            );

            scope.$on('$destroy', function () {
                stopWatch();
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valServer", valServer);
