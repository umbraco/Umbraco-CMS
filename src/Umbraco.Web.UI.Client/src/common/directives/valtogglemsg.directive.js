/**
    * @ngdoc directive 
    * @name umbraco.directive:valToggleMsg
    * @restrict A
    * @description This directive will show/hide an error based on: is the value + the given validator invalid? AND, has the form been submitted ?
    **/
function valToggleMsg(umbFormHelper) {
    return {
        restrict: "A",
        link: function (scope, element, attr, ctrl) {

            if (!attr.valToggleMsg)
                throw "valToggleMsg requires that a reference to a validator is specified";
            if (!attr.valMsgFor)
                throw "valToggleMsg requires that the attribute valMsgFor exists on the element";

            //create a flag for us to be able to reference in the below closures for watching.
            var showValidation = false;
            var hasError = false;

            var currentForm = umbFormHelper.getCurrentForm(scope);
            if (!currentForm || !currentForm.$name)
                throw "valToggleMsg requires that a name is assigned to the ng-form containing the validated input";

            //add a watch to the validator for the value (i.e. $parent.myForm.value.$error.required )
            scope.$watch(currentForm.$name + "." + attr.valMsgFor + ".$error." + attr.valToggleMsg, function (isInvalid, oldValue) {
                hasError = isInvalid;
                if (hasError && showValidation) {
                    element.show();
                }
                else {
                    element.hide();
                }
            });

            //add a watch to update our waitingOnValidation flag for use in the above closure
            scope.$watch("$parent.ui.waitingOnValidation", function (isWaiting, oldValue) {
                showValidation = isWaiting;
                if (hasError && showValidation) {
                    element.show();
                }
                else {
                    element.hide();
                }
            });
        }
    };
}
angular.module('umbraco.directives').directive("valToggleMsg", valToggleMsg);