(function () {
    "use strict";

    /**
     * @ngdoc component
     * @name Umbraco.umbBlockListScopedBlockContent
     * @function
     *
     * @description
     * The component for a style-scoped block of the block list property editor.
     */
    
    angular
        .module("umbraco")
        .component("umbBlockListScopedBlockContent", {
            controller: BlockListScopedBlockContentController,
            controllerAs: "model",
            bindings: {
                stylesheet: "@",
                view: "@",
                block: "=",
                api: "=",
                index: "<"
            }
        }
    );

    function BlockListScopedBlockContentController($compile, $element, $scope) {
        var vm = this;
        vm.$onInit = function() {
            $scope.block = vm.block;
            $scope.api = vm.api;
            var shadowRoot = $element[0].attachShadow({mode:'open'});
            shadowRoot.innerHTML = `
                <style>
                @import "${vm.stylesheet}"
                </style>
                <div ng-include="'${vm.view}'"></div>
            `;
            $compile(shadowRoot)($scope);
        }
    }
        

})();
