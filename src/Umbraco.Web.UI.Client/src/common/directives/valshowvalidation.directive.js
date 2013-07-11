
/**
* @ngdoc object 
* @name umbraco.directive:valShowValidation
* @restrict A
* @description Used to toggle the show-validation class on the element containing the form elements to validate.
*   This is used because we don't want to show validation messages until after the form is submitted and then reset
*   the process when the form is successful. We do this by listening to the current controller's saving and saved events.
**/
function valShowValidation() {
    return {
        require: "ngController",
        restrict: "A",
        link: function (scope, element, attr, ctrl) {
            //listen for the forms saving event
            scope.$on("saving", function (ev, args) {
                element.addClass("show-validation");
            });
            //listen for the forms saved event
            scope.$on("saved", function (ev, args) {
                element.removeClass("show-validation");
            });
        }
    };
}
angular.module('umbraco.directives').directive("valShowValidation", valShowValidation);