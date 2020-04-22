(function () {
    "use strict";


    /**
     * @ngdoc component
     * @name Umbraco.umbBlockListBlockContent
     * @function
     *
     * @description
     * The component for a style-inheriting block of the block list property editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockListBlockContent", {
            template: '<div ng-include="vm.view"></div>',
            controller: BlockListBlockContentController,
            controllerAs: "vm",
            bindings: {
                view: "@",
                block: "=",
                api: "="
            }
        }
    );

    function BlockListBlockContentController($scope) {
        var vm = this;
        vm.$onInit = function() {
            $scope.block = vm.block;
            $scope.api = vm.api;
        };
    }
        

})();
