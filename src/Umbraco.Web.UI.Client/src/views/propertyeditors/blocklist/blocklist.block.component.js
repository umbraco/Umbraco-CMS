(function () {
    "use strict";

    /**
     * @ngdoc component
     * @name umbraco.directives.directive:umbBlockListBlockContent
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
                api: "=",
                index: "<"
            }
        }
    );

    function BlockListBlockContentController($scope) {
        var model = this;
        model.$onInit = function() {
            $scope.block = model.block;
            $scope.api = model.api;
        };
    }
        

})();
