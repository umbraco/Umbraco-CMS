function valToggleMsg(serverValidationManager) {
    return {
        require: "^form",
        restrict: "A",

        /**
            Our directive requries a reference to a form controller which gets passed in to this parameter
         */
        link: function (scope, element, attr, formCtrl) {

            if (!attr.valToggleMsg){
                throw "valToggleMsg requires that a reference to a validator is specified";
            }
            if (!attr.valMsgFor){
                throw "valToggleMsg requires that the attribute valMsgFor exists on the element";
            }

            //if there's any remaining errors in the server validation service then we should show them.
            var showValidation = serverValidationManager.items.length > 0;
            var hasError = false;

            //add a watch to the validator for the value (i.e. myForm.value.$error.required )
            scope.$watch(formCtrl.$name + "." + attr.valMsgFor + ".$error." + attr.valToggleMsg, function (isInvalid, oldValue) {
                hasError = isInvalid;
                if (hasError && showValidation) {
                    element.show();
                }
                else {
                    element.hide();
                }
            });
            
            scope.$on("saving", function(ev, args) {
                showValidation = true;
                if (hasError) {
                    element.show();
                }
                else {
                    element.hide();
                }
            });
            
            scope.$on("saved", function (ev, args) {
                showValidation = false;
                element.hide();
            });

        }
    };
}

/**
* @ngdoc directive
* @name umbraco.directives.directive:valToggleMsg
* @restrict A
* @element input
* @requires formController
* @description This directive will show/hide an error based on: is the value + the given validator invalid? AND, has the form been submitted ?
**/
angular.module('umbraco.directives').directive("valToggleMsg", valToggleMsg);