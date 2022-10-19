(function () {
    'use strict';

    /**
     * A component to render the property action toggle
     */
    
    function umbPropertyActionsController(keyboardService, localizationService) {

        var vm = this;

        vm.isOpen = false;
        vm.labels = {
            openText: "Open Property Actions",
            closeText: "Close Property Actions"
        };

        vm.open = open;
        vm.close = close;
        vm.toggle = toggle;
        vm.executeAction = executeAction;

        vm.$onDestroy = onDestroy;
        vm.$onInit = onInit;
        vm.$onChanges = onChanges;

        function initDropDown() {
            keyboardService.bind("esc", vm.close);
        }

        function destroyDropDown() {
            keyboardService.unbind("esc");
        }

        function toggle() {
            if (vm.isOpen === true) {
                vm.close();
            } else {
                vm.open();
            }
        }

        function open() {
            vm.isOpen = true;
            initDropDown();
        }

        function close() {
            vm.isOpen = false;
            destroyDropDown();
        }

        function executeAction(action) {
            action.method();
            vm.close();
        }

        function onDestroy() {
            if (vm.isOpen === true) {
                destroyDropDown();
            }
        }

        function onInit() {
            
            var labelKeys = [
                "propertyActions_tooltipForPropertyActionsMenu",
                "propertyActions_tooltipForPropertyActionsMenuClose"
            ]

            localizationService.localizeMany(labelKeys).then(values => {
                vm.labels.openText = values[0];
                vm.labels.closeText = values[1];
            });
        }

        function onChanges(simpleChanges) {
            if (simpleChanges.actions) {

              let actions = simpleChanges.actions.currentValue || [];

              Utilities.forEach(actions, action => {

                if (action.labelKey) {
                    localizationService.localize(action.labelKey, (action.labelTokens || []), action.label).then(data => {
                      action.label = data;
                    });
                    
                    action.useLegacyIcon = action.useLegacyIcon === false ? false : true;
                    action.icon = (action.useLegacyIcon ? 'icon-' : '') + action.icon;
                }
              });
            }
        }
    }

    var umbPropertyActionsComponent = {
        templateUrl: 'views/components/property/umb-property-actions.html',
        bindings: {
            actions: "<"
        },
        controllerAs: 'vm',
        controller: umbPropertyActionsController
    };

    angular.module('umbraco.directives').component('umbPropertyActions', umbPropertyActionsComponent);

})();
