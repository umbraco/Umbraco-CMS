/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives.html")
    .directive('umbControlGroup', function (localizationService) {
        return {
            scope: {
                label: "@label",
                description: "@",
                hideLabel: "@",
                alias: "@"
            },
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/html/umb-control-group.html',
            link: function (scope, element, attr){
                if(scope.label && scope.label[0] === "@"){
                        scope.labelstring = localizationService.localize(scope.label.substring(1));
                }else{
                    scope.labelstring = scope.label;
                }
            }
        };
    });