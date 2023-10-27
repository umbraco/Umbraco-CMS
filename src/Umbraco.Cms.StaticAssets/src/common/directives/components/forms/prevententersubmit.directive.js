/**
* @ngdoc directive
* @name umbraco.directives.directive:preventEnterSubmit
* @description prevents a form from submitting when the enter key is pressed on an input field
**/
angular.module("umbraco.directives")
    .directive('preventEnterSubmit', function() {
        return function(scope, element, attrs) {

            var enabled = true;
            //check if there's a value for the attribute, if there is and it's false then we conditionally don't 
            //prevent default.
            if (attrs.preventEnterSubmit) {
                attrs.$observe("preventEnterSubmit", function (newVal) {
                    enabled = (newVal === "false" || newVal === 0 || newVal === false) ? false : true;
                });
            }

            $(element).on("keypress", function (event) {
                if (event.which === 13 && enabled === true) {
                    event.preventDefault();
                }
            });
        };
    });