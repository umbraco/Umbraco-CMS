(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridRoot
     * @description
     * The component to render the view for a block grid in the Property Editor.
     * Creates a ShadowDom for the layout.
     */

    angular
        .module("umbraco")
        .component("umbBlockGridRoot", {
            controller: umbBlockGridRootController,
            controllerAs: "vm",
            bindings: {
                stylesheet: "@",
                blockEditorApi: "<",
                entries: "<"
            }
        }
    );

    function umbBlockGridRootController($scope, $compile, $element) {
        var vm = this;

        vm.$onInit = function () {

            //$scope.valFormManager = vm.valFormManager;

            var shadowRoot = $element[0].attachShadow({ mode: 'open' });
            shadowRoot.innerHTML = 
            `
                <style>
                    @import '{{vm.stylesheet}}';
                    @import 'assets/css/blockgridui.css';
                    :host {
                        --umb-block-grid--grid-columns: 12;
                    }
                </style>
                <umb-block-grid-entries
                    block-editor-api="vm.blockEditorApi"
                    entries="vm.entries">
                </umb-block-grid-entries>
            `;
            $compile(shadowRoot)($scope);
            
        };
    }


})();
