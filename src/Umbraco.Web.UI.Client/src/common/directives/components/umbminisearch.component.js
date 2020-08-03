(function () {
    'use strict';

    angular
        .module('umbraco')
        .component('umbMiniSearch', {
            templateUrl: 'views/components/umb-mini-search.html',
            controller: UmbMiniSearchController,
            controllerAs: 'vm',
            bindings: {
                model: "=",
                onStartTyping: "&?",
                onSearch: "&?",
                onBlur: "&?"
            }
        });

    function UmbMiniSearchController($scope) {
        
        var vm = this;

        vm.onKeyDown = onKeyDown;
        vm.onChange = onChange;
        
        var searchDelay = _.debounce(function () {
            $scope.$apply(function () {
                if (vm.onSearch) {
                    vm.onSearch();
                }
            });
        }, 500);
    
        function onKeyDown(evt) {
            //13: enter
            switch (evt.keyCode) {
                case 13:
                    if (vm.onSearch) {
                        vm.onSearch();
                    }
                    break;
            }
        }
    
        function onChange() {
            if (vm.onStartTyping) {
                vm.onStartTyping();
            }
            searchDelay();
        }

    }

})();
