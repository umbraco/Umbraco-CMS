(function () {
    "use strict";

    /**
     * @ngdoc component
     * @name umbraco.directives.directive:umbBlockListScopedBlockContent
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
