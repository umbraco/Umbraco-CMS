(function () {
    'use strict';

    /**
     * A component to render the property action toggle
     */
    
    function umbPropertyActionsController(keyboardService) {

        var vm = this;

        vm.isOpen = false;

        function initDropDown() {
            keyboardService.bind("esc", vm.close);
        }
        function destroyDropDown() {
            keyboardService.unbind("esc");
        }

        vm.toggle = function() {
            if (vm.isOpen === true) {
                vm.close();
            } else {
                vm.open();
            }
        }
        vm.open = function() {
            vm.isOpen = true;
            initDropDown();
        }
        vm.close = function() {
            vm.isOpen = false;
            destroyDropDown();
        }

        vm.executeAction = function(action) {
            action.method();
            vm.close();
        }

        vm.$onDestroy = function () {
            if (vm.isOpen === true) {
                destroyDropDown();
            }
        }
        
    }

    var umbPropertyActionsComponent = {
        templateUrl: 'views/components/property/property-actions/umb-property-actions.html',
        bindings: {
            actions: "<"
        },
        controllerAs: 'vm',
        controller: umbPropertyActionsController
    };

    angular.module('umbraco.directives').component('umbPropertyActions', umbPropertyActionsComponent);

})();
