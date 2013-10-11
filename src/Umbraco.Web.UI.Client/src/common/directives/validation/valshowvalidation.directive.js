/**
* @ngdoc directive
* @name umbraco.directives.directive:valShowValidation
* @restrict A
* @description Used to toggle the show-validation class on the element containing the form elements to validate.
*   This is used because we don't want to show validation messages until after the form is submitted and then reset
*   the process when the form is successful. We do this by listening to the current controller's saving and saved events.
**/
function valShowValidation(serverValidationManager) {
    return {
        restrict: "A",
        link: function (scope, element, attr, ctrl) {

            var className = attr.valShowValidation ? attr.valShowValidation : "show-validation";
            var savingEventName = attr.savingEvent ? attr.savingEvent : "formSubmitting";
            var savedEvent = attr.savedEvent ? attr.savingEvent : "formSubmitted";

            //we should show validation if there are any msgs in the server validation collection
            if (serverValidationManager.items.length > 0) {
                element.addClass(className);
            }

            //listen for the forms saving event
            scope.$on(savingEventName, function (ev, args) {
                element.addClass(className);
            });
            
            //listen for the forms saved event
            scope.$on(savedEvent, function (ev, args) {
                element.removeClass(className);
            });
            
        }
    };
}
angular.module('umbraco.directives').directive("valShowValidation", valShowValidation);