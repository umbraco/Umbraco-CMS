/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
angular.module("umbraco.directives")
    .directive('umbProperty', function (userService, serverValidationManager, udiService) {

        // if only a guid is passed in, we'll ensure a correct udi structure
        function ensureUdi(udi) {
            if (udi && !udi.startsWith("umb://")) {
                udi = udiService.build("element", udi);
            }
            return udi;
        }

        return {
            scope: {
                property: "=",                
                elementUdi: "@",
                // optional, if set this will be used for the property alias validation path (hack required because NC changes the actual property.alias :/)
                propertyAlias: "@", 
                showInherit: "<",
                inheritsFrom: "<"
            },
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/property/umb-property.html',
            link: function (scope, element, attr, ctrls) {

                scope.controlLabelTitle = null;
                if(Umbraco.Sys.ServerVariables.isDebuggingEnabled) {
                    userService.getCurrentUser().then(function (u) {
                        if(u.allowedSections.indexOf("settings") !== -1 ? true : false) {
                            scope.controlLabelTitle = scope.property.alias;
                        }
                    });
                }

                scope.elementUdi = ensureUdi(scope.elementUdi);

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

                // returns the unique Id for the property to be used as the validation key for server side validation logic
                self.getValidationPath = function () {

                    // the elementUdi will be empty when this is not a nested property
                    var propAlias = $scope.propertyAlias ? $scope.propertyAlias : $scope.property.alias;
                    $scope.elementUdi = ensureUdi($scope.elementUdi);
                    return serverValidationManager.createPropertyValidationKey(propAlias, $scope.elementUdi);
                }
                $scope.getValidationPath = self.getValidationPath;

            }
        };
    });
