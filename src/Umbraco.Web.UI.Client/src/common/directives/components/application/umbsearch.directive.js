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
        vm.handleKeyDown = handleKeyDown;
        vm.closeSearch = closeSearch;
        vm.focusSearch = focusSearch;
        
        //we need to capture the focus before this element is initialized.
        vm.focusBeforeOpening = focusService.getLastKnownFocus();

        vm.activeResult = null;
        vm.activeResultGroup = null;

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
        function handleKeyDown(event) {
            
            // esc
            if(event.keyCode === 27) {
                event.stopPropagation();
                event.preventDefault();
                
                closeSearch();
                return;            
            }

            // up/down (navigate search results)
            if (vm.hasResults && (event.keyCode === 38 || event.keyCode === 40)) {
                event.stopPropagation();
                event.preventDefault();

                var allGroups = _.values(vm.searchResults);
                var down = event.keyCode === 40;
                if (vm.activeResultGroup === null) {
                    // it's the first time navigating, pick the appropriate group and result
                    // - first group and first result when navigating down
                    // - last group and last result when navigating up
                    vm.activeResultGroup = down ? allGroups[0] : allGroups[allGroups.length - 1];
                    vm.activeResult = down ? vm.activeResultGroup.results[0] : vm.activeResultGroup.results[vm.activeResultGroup.results.length - 1];
                }
                else if (down) {
                    // handle navigation down through the groups and results
                    if (vm.activeResult === vm.activeResultGroup.results[vm.activeResultGroup.results.length - 1]) {
                        if (vm.activeResultGroup === allGroups[allGroups.length - 1]) {
                            vm.activeResultGroup = allGroups[0];
                        }
                        else {
                            vm.activeResultGroup = allGroups[allGroups.indexOf(vm.activeResultGroup) + 1];
                        }
                        vm.activeResult = vm.activeResultGroup.results[0];
                    }
                    else {
                        vm.activeResult = vm.activeResultGroup.results[vm.activeResultGroup.results.indexOf(vm.activeResult) + 1];
                    }
                }
                else {
                    // handle navigation up through the groups and results
                    if (vm.activeResult === vm.activeResultGroup.results[0]) {
                        if (vm.activeResultGroup === allGroups[0]) {
                            vm.activeResultGroup = allGroups[allGroups.length - 1];
                        }
                        else {
                            vm.activeResultGroup = allGroups[allGroups.indexOf(vm.activeResultGroup) - 1];
                        }
                        vm.activeResult = vm.activeResultGroup.results[vm.activeResultGroup.results.length - 1];
                    }
                    else {
                        vm.activeResult = vm.activeResultGroup.results[vm.activeResultGroup.results.indexOf(vm.activeResult) - 1];
                    }
                }

                $timeout(function () {
                    var resultElementLink = angular.element(".umb-search-item[active-result='true'] .umb-search-result__link");
                    resultElementLink[0].focus();
                });
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
