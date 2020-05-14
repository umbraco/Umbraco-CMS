(function () {
    'use strict';

    angular
        .module('umbraco')
        .component('umbMiniSearch', {
            templateUrl: 'views/components/umb-mini-search/umb-mini-search.html',
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
        
        var searchDelay = _.debounce(function () {
            $scope.$apply(function () {
                if (vm.onSearch) {
                    vm.onSearch();
                }
            });
        }, 500);
    
        vm.onKeyDown = function (ev) {
            //13: enter
            switch (ev.keyCode) {
                case 13:
                    if (vm.onSearch) {
                        vm.onSearch();
                    }
                    break;
            }
        };
    
        vm.onChange = function () {
            if (vm.onStartTyping) {
                vm.onStartTyping();
            }
            searchDelay();
        };

    }

})();
