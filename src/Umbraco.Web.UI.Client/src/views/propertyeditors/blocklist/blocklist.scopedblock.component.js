(function () {
    "use strict";

    /**
     * @ngdoc component
     * @name umbraco.directives.directive:umbBlockListScopedBlock
     * @description
     * The component for a style-scoped block of the block list property editor.
     */
    
    angular
        .module("umbraco")
        .component("umbBlockListScopedBlock", {
            controller: BlockListScopedBlockController,
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

    function BlockListScopedBlockController($compile, $element, $scope) {
        var model = this;
        model.$onInit = function() {
            $scope.block = model.block;
            $scope.api = model.api;
            var shadowRoot = $element[0].attachShadow({mode:'open'});
            shadowRoot.innerHTML = `
                <style>
                @import "${model.stylesheet}"
                </style>
                <div ng-include="'${model.view}'"></div>
            `;
            $compile(shadowRoot)($scope);
        }
    }
        

})();
