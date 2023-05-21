/**
 * @ngdoc directive
 * @name umbraco.directives.directive:valServerMatch
 * @restrict A
 * @description A custom validator applied to a form/ng-form within an umbProperty that validates server side validation data
 * contained within the serverValidationManager. The data can be matched on "exact", "prefix", "suffix" or "contains" matches against
 * a property validation key. The attribute value can be in multiple value types:
 * - STRING = The property validation key to have an exact match on. If matched, then the form will have a valServerMatch validator applied.
 * - OBJECT = A dictionary where the key is the match type: "contains", "prefix", "suffix" and the value is either:
 *     - ARRAY = A list of property validation keys to match on. If any are matched then the form will have a valServerMatch validator applied.
 *     - OBJECT = A dictionary where the key is the validator error name applied to the form and the value is the STRING of the property validation key to match on
**/
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

            function bindCallback(validationKey, matchVal, matchType) {

                if (!matchVal) return;

                if (Utilities.isString(matchVal)) {
                    matchVal = [matchVal]; // normalize to an array since the value can also natively be an array
                }

                // match for each string in the array
                matchVal.forEach(m => {
                    unsubscribe.push(serverValidationManager.subscribe(
                        m,
                        currentCulture,
                        "",
                        // the callback
                        function (isValid, propertyErrors, allErrors) {
                            if (!isValid) {
                                formCtrl.$setValidity(validationKey, false);
                            }
                            else {
                                formCtrl.$setValidity(validationKey, true);
                            }
                        },
                        currentSegment,
                        matchType ? { matchType: matchType } : null // specify the match type
                    ));
                });

            }

            if (Utilities.isObject(scope.valServerMatch)) {
                var allowedKeys = ["contains", "prefix", "suffix"];
                Object.keys(scope.valServerMatch).forEach(matchType => {
                    if (allowedKeys.indexOf(matchType) === -1) {
                        throw "valServerMatch dictionary keys must be one of " + allowedKeys.join();
                    }

                    var matchVal = scope.valServerMatch[matchType];

                    if (Utilities.isObject(matchVal)) {

                        // as an object, the key will be the validation error instead of the default "valServerMatch"
                        Object.keys(matchVal).forEach(valKey => {

                            // matchVal[valKey] can be an ARRAY or a STRING
                            bindCallback(valKey, matchVal[valKey], matchType);
                        });
                    }
                    else {

                        // matchVal can be an ARRAY or a STRING
                        bindCallback("valServerMatch", matchVal, matchType);
                    }
                });
            }
            else if (Utilities.isString(scope.valServerMatch)) {

                // a STRING match which will be an exact match on the string supplied as the property validation key
                bindCallback("valServerMatch", scope.valServerMatch, null);
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
