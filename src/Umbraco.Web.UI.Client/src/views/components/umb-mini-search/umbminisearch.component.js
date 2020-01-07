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
                onStartTyping: "&",
                onSearch: "&"
            }
        });

    function UmbMiniSearchController($scope) {
        
        var vm = this;
        
        var searchDelay = _.debounce(function () {
            $scope.$apply(function () {
                vm.onSearch();
            });
        }, 500);
    
        vm.onKeyDown = function (ev) {
            //13: enter
            switch (ev.keyCode) {
                case 13:
                    vm.onSearch();
                    break;
            }
        };
    
        vm.onChange = function () {
            vm.onStartTyping();
            searchDelay();
        };

    }

})();
