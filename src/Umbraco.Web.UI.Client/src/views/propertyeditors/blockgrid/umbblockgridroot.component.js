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
                gridColumns: "@",
                createLabel: "@",
                stylesheet: "@",
                blockEditorApi: "<",
                propertyEditorForm: "<?",
                entries: "<",
                loading: "<"
            }
        }
    );

    function umbBlockGridRootController($scope, $compile, $element) {
        var vm = this;

        vm.$onInit = function () {
            
            var shadowRoot = $element[0].attachShadow({ mode: 'open' });
            shadowRoot.innerHTML = 
            `
                <style>
                    {{vm.stylesheet ? "@import '" + vm.stylesheet + "?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}';" : ""}}
                    @import 'assets/css/blockgridui.css?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}';
                    :host {
                        --umb-block-grid--grid-columns: ${vm.gridColumns};
                    }
                </style>
                <div 
                    ng-if="vm.loading !== true"
                    class="umb-block-grid"
                    ng-class="{'show-validation': vm.blockEditorApi.internal.showValidation}"
                    data-grid-columns="${vm.gridColumns}"
                >
                    <umb-block-grid-entries
                        block-editor-api="vm.blockEditorApi"
                        entries="vm.entries"
                        layout-columns="vm.gridColumns"
                        property-editor-form="vm.propertyEditorForm"
                        depth="0"
                        create-label="{{::vm.createLabel}}">
                    </umb-block-grid-entries>
                </div>
            `;
            $compile(shadowRoot)($scope);
            
        };
    }


})();
