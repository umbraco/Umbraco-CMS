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
    
    function umbSearchController($timeout, backdropService, searchService, focusService) {

        var vm = this;

        vm.$onInit = onInit;
        vm.$onDestroy = onDestroy;
        vm.search = search;
        vm.clickItem = clickItem;
        vm.clearSearch = clearSearch;
        vm.handleKeyUp = handleKeyUp;
        vm.closeSearch = closeSearch;
        vm.focusSearch = focusSearch;
        
        //we need to capture the focus before this element is initialized.
        vm.focusBeforeOpening = focusService.lastKnownFocus;

        function onInit() {
            vm.searchQuery = "";
            vm.searchResults = [];
            vm.hasResults = false;
            focusSearch();
            backdropService.open();
        }

        function onDestroy() {
            backdropService.close();
        }

        /**
         * Handles when a search result is clicked
         */
        function clickItem() {
            closeSearch();
        }

        /**
         * Clears the search query
         */
        function clearSearch() {
            vm.searchQuery = "";
            vm.searchResults = [];
            vm.hasResults = false;
            focusSearch();
        }

        /**
         * Add focus to the search field
         */
        function focusSearch() {
            vm.searchHasFocus = false;
            $timeout(function(){
                vm.searchHasFocus  = true;
            });
        }

        /**
         * Handles all keyboard events
         * @param {object} event
         */
        function handleKeyUp(event) {
            
            event.stopPropagation();
            event.preventDefault();
            
            // esc
            if(event.keyCode === 27) {
                closeSearch();
            }
        }

        /**
         * Used to proxy a callback
         */
        function closeSearch() {
            if(vm.focusBeforeOpening) {
                vm.focusBeforeOpening.focus();
            }
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
                clearSearch();
            }
        }

    }

    angular.module('umbraco.directives').component('umbSearch', umbSearch);

})();
