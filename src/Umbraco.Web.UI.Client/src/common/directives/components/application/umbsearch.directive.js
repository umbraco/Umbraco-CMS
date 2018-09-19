(function () {
    'use strict';

    /**
     * A component to render the pop up search field
     */
    var umbSearch = {
        templateUrl: 'views/components/application/umb-search.html',
        controllerAs: 'vm',
        controller: umbSearchController,
        bindings: {
            onClose: "&"
        }
    };
    
    function umbSearchController($scope, backdropService) {

        var vm = this;

        vm.searchResults = [];

        vm.$onInit = onInit;
        vm.search = search;
        vm.closeSearch = closeSearch;

        function onInit() {
            console.log("init search thingy");
            backdropService.open();
        }

        /**
         * Used to proxy a callback
         */
        function closeSearch() {
            if(vm.onClose) {
                vm.onClose();
            }
        }

        /**
         * Used to search
         * @param {string} searchQuery
         */
        function search(searchQuery) {
            console.log(searchQuery);
        }

    }

    angular.module('umbraco.directives').component('umbSearch', umbSearch);

})();