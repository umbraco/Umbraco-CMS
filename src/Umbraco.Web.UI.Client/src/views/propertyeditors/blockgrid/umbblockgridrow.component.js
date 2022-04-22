(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridRow
     * @description
     * renders each row for the block grid editor
     */
    
    angular
        .module("umbraco")
        .component("umbBlockGridRow", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-row.html',
            controller: BlockGridRowController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                layout: "<",
                index: "<"
            },
            require: {
                valFormManager: "^^valFormManager"
            }
        }
    );

    function BlockGridRowController($scope) {

        var vm = this;

        vm.$onInit = function () {
          
        };
    }   

})();
