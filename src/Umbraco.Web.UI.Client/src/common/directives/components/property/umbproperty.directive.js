/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbProperty', function (userService) {
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

                scope.showPropertyAlias = false;
                
                userService.getCurrentUser().then(function (u) {
                    var hasAccessToSettings = u.allowedSections.indexOf("settings") !== -1 ? true : false;

                    scope.showPropertyAlias = (hasAccessToSettings && Umbraco.Sys.ServerVariables.isDebuggingEnabled)
                    
                });
            },
            //Define a controller for this directive to expose APIs to other directives
            controller: function ($scope) {

                var self = this;
                
                //set the API properties/methods

                self.property = $scope.property;
                self.setPropertyError = function (errorMsg) {
                    $scope.property.propertyErrorMessage = errorMsg;
                };

                $scope.propertyActions = [];
                self.setPropertyActions = function(actions) {
                    $scope.propertyActions = actions;
                };

            }
        };
    });
