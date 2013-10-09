/**
* @ngdoc directive
* @name umbraco.directives.directive:valStatusChanged
* @restrict A
* @description Used to broadcast an event to all elements inside this one to notify that form validation has 
* changed. If we don't use this that means you have to put a watch for each directive on a form's validation
* changing which would result in much higher processing. We need to actually watch the whole $error collection of a form
* because just watching $valid or $invalid doesn't acurrately trigger form validation changing.
**/
function valStatusChanged() {
    return {
        require: "form",
        restrict: "A",
        link: function (scope, element, attr, formCtrl) {

            scope.$watch(function () {
                return formCtrl.$error;
            }, function () {
                scope.$broadcast("valStatusChanged", { form: formCtrl });
            }, true);
        }
    };
}
angular.module('umbraco.directives').directive("valStatusChanged", valStatusChanged);