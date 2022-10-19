/**
* @ngdoc directive
* @name umbraco.directives.directive:umbProperty
* @restrict E
**/
(function () {
    'use strict';

    angular
        .module("umbraco.directives")
        .component('umbProperty', {
            templateUrl: 'views/components/property/umb-property.html',
            controller: UmbPropertyController,
            controllerAs: 'vm',
            transclude: true,
            require: {
                parentUmbProperty: '?^^umbProperty',
                parentForm: '?^^form'
            },
            bindings: {
                property: "=",
                node: "<",
                elementKey: "@",
                // optional, if set this will be used for the property alias validation path (hack required because NC changes the actual property.alias :/)
                propertyAlias: "@",
                showInherit: "<",
                inheritsFrom: "<"
            }
        });

    

    function UmbPropertyController($scope, userService, serverValidationManager, udiService, angularHelper) {

        const vm = this;

        vm.$onInit = onInit;

        vm.setDirty = function () {
            // NOTE: We need to use scope because we haven't changd it to vm.propertyForm in the html and that
            // might mean a breaking change.
            $scope.propertyForm.$setDirty();
        }

        vm.setPropertyError = function (errorMsg) {
            vm.property.propertyErrorMessage = errorMsg;
        };

        vm.propertyActions = [];
        vm.setPropertyActions = function (actions) {
            vm.propertyActions = actions;
        };

        // returns the validation path for the property to be used as the validation key for server side validation logic
        vm.getValidationPath = function () {

            var parentValidationPath = vm.parentUmbProperty ? vm.parentUmbProperty.getValidationPath() : null;            
            var propAlias = vm.propertyAlias ? vm.propertyAlias : vm.property.alias;
            // the elementKey will be empty when this is not a nested property
            var valPath = vm.elementKey ? vm.elementKey + "/" + propAlias : propAlias;
            return serverValidationManager.createPropertyValidationKey(valPath, parentValidationPath);
        }

        function onInit() {
            vm.controlLabelTitle = null;
            if (Umbraco.Sys.ServerVariables.isDebuggingEnabled) {
                userService.getCurrentUser().then(function (u) {
                    if (u.allowedSections.indexOf("settings") !== -1 ? true : false) {
                        vm.controlLabelTitle = vm.property.alias;
                    }
                });
            }

            if (!vm.parentUmbProperty) {
                // not found, then fallback to searching the scope chain, this may be needed when DOM inheritance isn't maintained but scope
                // inheritance is (i.e.infinite editing)
                var found = angularHelper.traverseScopeChain($scope, s => s && s.vm && s.vm.constructor.name === "UmbPropertyController");
                vm.parentUmbProperty = found ? found.vm : null;
          }

          if (vm.property.description) {
            // split by lines containing only '--'
            var descriptionParts = vm.property.description.split(/^--$/gim);
            if (descriptionParts.length > 1) {
              // if more than one part, we have an extended description,
              // combine to one extended description, and remove leading linebreak
              vm.property.extendedDescription = descriptionParts.splice(1).join("--").substring(1);
              vm.property.extendedDescriptionVisible = false;

              // set propertydescription to first part, and remove trailing linebreak
              vm.property.description = descriptionParts[0].slice(0, -1);
            }
          }
        }

    }

})();
