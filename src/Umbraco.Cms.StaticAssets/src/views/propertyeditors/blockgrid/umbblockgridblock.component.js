(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridBlock
     * @description
     * The component to render the view for a block in the Block Grid Editor.
     * If a stylesheet is used then this uses a ShadowDom to make a scoped element.
     * This way the backoffice styling does not collide with the block style.
     */

    angular
        .module("umbraco")
        .component("umbBlockGridBlock", {
            controller: BlockGridBlockController,
            controllerAs: "model",
            bindings: {
                stylesheet: "@",
                view: "@",
                block: "=",
                api: "<",
                index: "<",
                parentForm: "<"
            },
            require: {
                valFormManager: "^^valFormManager"
            }
        }
    );

    function BlockGridBlockController($scope, $compile, $element) {
        var model = this;

        model.$onInit = function () {

            // let the Block know about its form
            model.block.setParentForm(model.parentForm);

            // let the Block know about the current index
            model.block.index = model.index;

            $scope.block = model.block;
            $scope.api = model.api;
            $scope.index = model.index;
            $scope.parentForm = model.parentForm;
            $scope.valFormManager = model.valFormManager;

            var shadowRoot = $element[0].attachShadow({ mode: 'open' });
            shadowRoot.innerHTML = 
            `
                <style>@import "assets/css/icons.css?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}"</style>
            
                ${ model.stylesheet ? `
                    <style>
                        @import "${model.stylesheet}?umb__rnd=${Umbraco.Sys.ServerVariables.application.cacheBuster}"
                    </style>`
                    : ''
                }
                
                <div 
                    style="display:contents;" 
                    ng-class="{'show-validation': vm.blockEditorApi.internal.showValidation}"
                    ng-include="api.internal.sortMode ? api.internal.sortModeView : '${model.view}'">
                </div>
            `;
            $compile(shadowRoot)($scope);
            
        };
        

        // We need to watch for changes on primitive types and update the $scope values.
        model.$onChanges = function (changes) {
            if (changes.index) {
                var index = changes.index.currentValue;
                $scope.index = index;

                // let the Block know about the current index:
                model.block.index = index;
                model.block.updateLabel();
            }
        };
        
    }


})();
