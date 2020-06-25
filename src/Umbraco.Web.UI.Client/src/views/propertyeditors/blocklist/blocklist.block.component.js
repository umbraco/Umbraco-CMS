(function () {
    "use strict";

    /**
     * @ngdoc component
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
                api: "=",
                index: "<"
            }
        }
    );

    function BlockListBlockController($scope) {
        var model = this;
        model.$onInit = function() {
            $scope.block = model.block;
            $scope.api = model.api;
        };
    }
        

})();
