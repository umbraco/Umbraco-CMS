/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbProperty', function (umbPropEditorHelper) {
        return {
            scope: {
                property: "="
            },
            transclude: true,
            restrict: 'E',
            replace: true,        
            templateUrl: 'views/directives/umb-property.html',

            //Define a controller for this directive to expose APIs to other directives
            controller: function ($scope, $timeout) {
               
                var self = this;

                //set the API properties/methods

                self.property = $scope.property;
                self.setPropertyError = function(errorMsg) {
                    $scope.property.propertyErrorMessage = errorMsg;
                };
            }
        };
    });