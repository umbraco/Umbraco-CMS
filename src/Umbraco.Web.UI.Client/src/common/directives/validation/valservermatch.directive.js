
function valServerMatch(serverValidationManager) {
    return {
        require: ['form', '^^umbProperty', '?^^umbVariantContent'],
        restrict: "A",
        scope: {
            valServerMatch: "="
        },
        link: function (scope, element, attr, ctrls) {

            var formCtrl = ctrls[0];
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
            
            var unsubscribe = [];

            //subscribe to the server validation changes
            function serverValidationManagerCallback(isValid, propertyErrors, allErrors) {
                if (!isValid) {
                    formCtrl.$setValidity('valServerMatch', false);                    
                }
                else {
                    formCtrl.$setValidity('valServerMatch', true);                    
                }
            }

            if (Utilities.isObject(scope.valServerMatch)) {
                var allowedKeys = ["contains", "prefix", "suffix"];
                Object.keys(scope.valServerMatch).forEach(k => {
                    if (allowedKeys.indexOf(k) === -1) {
                        throw "valServerMatch dictionary keys must be one of " + allowedKeys.join();
                    }

                    unsubscribe.push(serverValidationManager.subscribe(
                        scope.valServerMatch[k],
                        currentCulture,
                        "",
                        serverValidationManagerCallback,
                        currentSegment,
                        { matchType: k } // specify the match type
                    ));

                });
            }
            else if (Utilities.isString(scope.valServerMatch)) {
                unsubscribe.push(serverValidationManager.subscribe(
                    scope.valServerMatch,
                    currentCulture,
                    "",
                    serverValidationManagerCallback,
                    currentSegment));
            }
            else {
                throw "valServerMatch value must be a string or a dictionary";
            }

            scope.$on('$destroy', function () {
                unsubscribe.forEach(u => u());
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("valServerMatch", valServerMatch);
