(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockListBlock
     * @description
     * The component for a style-inheriting block of the block list property editor.
     */
    
    angular
        .module("umbraco")
        .component("umbBlockListBlock", {
            template: '<div ng-include="model.view"></div>',
            controller: BlockListBlockController,
            controllerAs: "model",
            bindings: {
                view: "@",
                block: "=",
                api: "<",
                index: "<",
                parentForm: "<"
            }
        }
    );

    function BlockListBlockController($scope) {
        var model = this;
        model.$onInit = function () {
            // Ugh, due to the way we work with angularjs and property editors not being components and needing to use ng-include,
            // it means we need to expose things directly on the $scope so they can use them.            

            $scope.block = model.block;
            $scope.api = model.api;
            $scope.index = model.index;
            $scope.parentForm = model.parentForm;
        };

        // We need to watch for changes on primitive types and upate the $scope values.
        model.$onChanges = function (changes) {
            if (changes.index) {
                $scope.index = changes.index.currentValue;
            }
        }
    }
        

})();
