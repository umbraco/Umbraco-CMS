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
            template: '<div ng-include="model.view"></div>',
            controller: BlockListBlockContentController,
            controllerAs: "model",
            bindings: {
                view: "@",
                block: "=",
                api: "=", // Should this be a one way bind?
                index: "<"
            }
        }
    );

    function BlockListBlockContentController($scope) {
        var model = this;
        model.$onInit = function () {
            // Ugh, due to the way we work with angularjs and property editors not being components and needing to use ng-include,
            // it means we need to expose things directly on the $scope so they can use them.
            // It also means we need to watch for changes and upate the $scope values.

            $scope.block = model.block;
            $scope.api = model.api;
            $scope.index = model.index;
        };
        model.$onChanges = function (changes) {
            if (changes.index) {
                $scope.index = changes.index.currentValue;
            }

            // TODO: Wouldn't we need to watch for any changes to model.block/api here too?
        }
    }
        

})();
