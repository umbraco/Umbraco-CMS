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
                onBlur: "&?",
                labelKey: "@?"
            }
        });

    function UmbMiniSearchController($scope, localizationService) {
        
        var vm = this;

        vm.onKeyDown = onKeyDown;
        vm.onChange = onChange;
        vm.inputId = randomUniquId();
        
        init();

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

        function randomUniquId() {
            return Math.floor((1 + Math.random()) * 0x10000)
                .toString(16)
                .substring(1);
        }

        function init() {
            setText();
        }

        function setText() {
            var keyToLocalize = vm.labelKey ? vm.labelKey : 'general_search';

            localizationService.localize(keyToLocalize).then(function (data) {
                // If a labelKey is passed let's update the returned text if it's does not contain an opening square bracket [
                    if(data.indexOf('[') === -1){
                    vm.text = data;
                    }
            });
        }
    }

})();
