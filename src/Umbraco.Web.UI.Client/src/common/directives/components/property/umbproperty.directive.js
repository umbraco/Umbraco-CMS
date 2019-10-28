/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbProperty', function (umbPropEditorHelper, userService) {
        return {
            scope: {
                property: "=",
                showInherit: "<",
                inheritsFrom: "<"
            },
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/property/umb-property.html',
            link: function (scope) {

                scope.propertyEditorAPI = {};

                userService.getCurrentUser().then(function (u) {
                    var isAdmin = u.userGroups.indexOf('admin') !== -1;
                    scope.propertyAlias = (Umbraco.Sys.ServerVariables.isDebuggingEnabled === true || isAdmin) ? scope.property.alias : null;
                });
            },
            //Define a controller for this directive to expose APIs to other directives
            controller: function ($scope, $timeout) {

                var self = this;

                //set the API properties/methods

                self.property = $scope.property;
                self.setPropertyError = function (errorMsg) {
                    $scope.property.propertyErrorMessage = errorMsg;
                };
                
                var unsubscribe = $scope.$on("ExposePropertyEditorAPI", function(event, api) {
                    
                    //avoid eventual parent properties to capture this.
                    event.stopPropagation();
                    
                    $scope.propertyEditorAPI = api;
                    //$scope.propertyActions = api.propertyActions;
                });

                $scope.$on("$destroy", function () {
                    unsubscribe();
                });

            }
        };
    });
