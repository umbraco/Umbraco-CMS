(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTabsNav', {
            transclude: true,
            templateUrl: "views/components/tabs/umb-tabs-nav.html",
            controller: UmbTabsNavController,
            controllerAs: 'vm',
            bindings: {
                tabs: "<",
                onTabChange: "&"
            }
        });

    function UmbTabsNavController(eventsService) {

        var vm = this;

        vm.clickTab = clickTab;

        function clickTab($event, tab) {
            if (vm.onTabChange) {
                var args = { "tab": tab, "tabs": vm.tabs };
                eventsService.emit("app.tabChange", args);
                vm.onTabChange({ "event": $event, "tab": tab });
            }
        }

    }

})();