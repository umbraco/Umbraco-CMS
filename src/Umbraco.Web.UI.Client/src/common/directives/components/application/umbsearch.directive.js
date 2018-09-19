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
    
    function umbSearchController($scope, backdropService, searchService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.$onDestroy = onDestroy;
        vm.search = search;
        vm.handleKeyUp = handleKeyUp;
        vm.closeSearch = closeSearch;

        function onInit() {
            vm.searchResults = [];
            vm.hasResults = false;
            backdropService.open();
        }

        function onDestroy() {
            backdropService.close();
        }

        function handleKeyUp(event) {
            // esc
            if(event.keyCode === 27) {
                closeSearch();
            }
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
            if(searchQuery.length > 0) {
                var search = {"term": searchQuery};
                searchService.searchAll(search).then(function(result){
                    //result is a dictionary of group Title and it's results
                    var filtered = {};
                    _.each(result, function (value, key) {
                        if (value.results.length > 0) {
                            filtered[key] = value;
                        }
                    });
                    // bind to view model
                    vm.searchResults = filtered;
                    // check if search has results
                    vm.hasResults = Object.keys(vm.searchResults).length > 0;
                });

            } else {
                vm.searchResults = [];
                vm.hasResults = false;
            }
        }

    }

    angular.module('umbraco.directives').component('umbSearch', umbSearch);

})();