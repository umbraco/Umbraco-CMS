(function () {
    'use strict';
    
    angular
        .module('umbraco.directives')
        .component('umbLayoutSelector', {
            templateUrl: 'views/components/umb-layout-selector.html',
            controller: LayoutSelectorController,
            controllerAs: 'vm',
            bindings: {
                layouts: '<',
                activeLayout: '<',
                onLayoutSelect: '&'
            }
        });

    function LayoutSelectorController($scope, $element) {

        var vm = this;

        vm.$onInit = onInit;

        vm.layoutDropDownIsOpen = false;
        vm.showLayoutSelector = true;
        vm.pickLayout = pickLayout;
        vm.toggleLayoutDropdown = toggleLayoutDropdown;
        vm.leaveLayoutDropdown = leaveLayoutDropdown;
        vm.closeLayoutDropdown = closeLayoutDropdown;

        function onInit() {
            activate();
        }

        function closeLayoutDropdown() {
            vm.layoutDropDownIsOpen = false;
        }

        function toggleLayoutDropdown() {
            vm.layoutDropDownIsOpen = !vm.layoutDropDownIsOpen;
        }

        function leaveLayoutDropdown() {
            vm.layoutDropDownIsOpen = false;
        }

        function pickLayout(selectedLayout) {
            if (vm.onLayoutSelect) {
                vm.onLayoutSelect({ layout: selectedLayout });
                vm.layoutDropDownIsOpen = false;
            }
        }

        function activate() {
            setVisibility();
            setActiveLayout(vm.layouts);
        }

        function setVisibility() {

            var numberOfAllowedLayouts = getNumberOfAllowedLayouts(vm.layouts);

            if (numberOfAllowedLayouts === 1) {
                vm.showLayoutSelector = false;
            }

        }

        function getNumberOfAllowedLayouts(layouts) {

            var allowedLayouts = 0;

            for (var i = 0; layouts.length > i; i++) {

                var layout = layouts[i];

                if (layout.selected === true) {
                    allowedLayouts++;
                }

            }

            return allowedLayouts;
        }

        function setActiveLayout(layouts) {

            for (var i = 0; layouts.length > i; i++) {
                var layout = layouts[i];
                if (layout.path === vm.activeLayout.path) {
                    layout.active = true;
                }
            }

        }
    }

})();
