/**
* @ngdoc directive
* @name umbraco.directives.directive:umbField
* @restrict E
*
* Used to constryct propety like form html, but without an actual property object
**/
/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbField', function (localizationService) {
        return {
            scope: {
                label: "@label",
                description: "@",
                hideLabel: "@",
                alias: "@"
            },
            require: '?^form',
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/components/field/umb-field.html',
            link: function (scope, element, attr, formCtrl) {

                scope.formValid = function() {
                    if (formCtrl) {
                        return formCtrl.$valid;
                    }
                    //there is no form.
                    return true;
                };

                if (scope.label && scope.label[0] === "@") {
                    scope.labelstring = localizationService.localize(scope.label.substring(1));
                }
                else {
                    scope.labelstring = scope.label;
                }

                if (scope.description && scope.description[0] === "@") {
                    scope.descriptionstring = localizationService.localize(scope.description.substring(1));
                }
                else {
                    scope.descriptionstring = scope.description;
                }

            }
        };
    });