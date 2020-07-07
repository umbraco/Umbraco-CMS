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
                elementUdi: "@",
                // optional, if set this will be used for the property alias validation path (hack required because NC changes the actual property.alias :/)
                propertyAlias: "@",
                showInherit: "<",
                inheritsFrom: "<"
            }
        });

    

    function UmbPropertyController($scope, userService, serverValidationManager, udiService, angularHelper) {

        const vm = this;

        vm.$onInit = onInit;

        vm.setPropertyError = function (errorMsg) {
            vm.property.propertyErrorMessage = errorMsg;
        };

        vm.propertyActions = [];
        vm.setPropertyActions = function (actions) {
            vm.propertyActions = actions;
        };

        // returns the unique Id for the property to be used as the validation key for server side validation logic
        vm.getValidationPath = function () {

            // the elementUdi will be empty when this is not a nested property
            var propAlias = vm.propertyAlias ? vm.propertyAlias : vm.property.alias;
            vm.elementUdi = ensureUdi(vm.elementUdi);
            return serverValidationManager.createPropertyValidationKey(propAlias, vm.elementUdi);
        }

        vm.getParentValidationPath = function () {
            if (!vm.parentUmbProperty) {
                return null;
            }
            return vm.parentUmbProperty.getValidationPath();
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

            vm.elementUdi = ensureUdi(vm.elementUdi);

            if (!vm.parentUmbProperty) {
                // not found, then fallback to searching the scope chain, this may be needed when DOM inheritance isn't maintained but scope
                // inheritance is (i.e.infinite editing)
                var found = angularHelper.traverseScopeChain($scope, s => s && s.vm && s.vm.constructor.name === "UmbPropertyController");
                vm.parentUmbProperty = found ? found.vm : null;
            }
        }

        // if only a guid is passed in, we'll ensure a correct udi structure
        function ensureUdi(udi) {
            if (udi && !udi.startsWith("umb://")) {
                udi = udiService.build("element", udi);
            }
            return udi;
        }
    }

})();
