(function () {
    "use strict";

    /**
     * @ngdoc directive
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
                api: "=", // Should this be a one way bind?
                index: "<"
            }
        }
    );

    function BlockListScopedBlockController($compile, $element, $scope) {
        var model = this;
        model.$onInit = function () {
            // Ugh, due to the way we work with angularjs and property editors not being components and needing to use ng-include,
            // it means we need to expose things directly on the $scope so they can use them.
            
            $scope.block = model.block;
            $scope.api = model.api;
            $scope.index = model.index;

            var shadowRoot = $element[0].attachShadow({mode:'open'});
            shadowRoot.innerHTML = `
                <style>
                @import "${model.stylesheet}"
                </style>
                <div ng-include="'${model.view}'"></div>
            `;
            $compile(shadowRoot)($scope);
        }

        // We need to watch for changes on primitive types and upate the $scope values.
        model.$onChanges = function (changes) {
            if (changes.index) {
                $scope.index = changes.index.currentValue;
            }
        }
    }
        

})();
