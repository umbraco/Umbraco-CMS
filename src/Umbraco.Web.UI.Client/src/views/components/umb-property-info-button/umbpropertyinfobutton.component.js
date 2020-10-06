(function () {
    'use strict';

    angular
        .module('umbraco')
        .component('umbPropertyInfoButton', {
            templateUrl: 'views/components/umb-property-info-button/umb-property-info-button.html',
            controller: UmbPropertyInfoButtonController,
            controllerAs: 'vm',
            transclude: true,
            bindings: {
                buttonTitle: "@?",
                symbol: "@?"
            }
        });

    function UmbPropertyInfoButtonController() {

        var vm = this;
        vm.show = false;

        vm.onMouseClick = function ($event) {
            vm.show = !vm.show;
        };
        vm.onMouseClickOutside = function ($event) {
            vm.show = false;
        };

        vm.$onInit = function() {
            vm.symbol = vm.symbol || "i";
        };

    }

})();
