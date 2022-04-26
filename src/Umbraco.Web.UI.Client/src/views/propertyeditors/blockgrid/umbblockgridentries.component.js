(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridEntries
     * @description
     * renders all blocks for a given list for the block grid editor
     */
    
    angular
        .module("umbraco")
        .component("umbBlockGridEntries", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-entries.html',
            controller: BlockGridEntriesController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                entries: "<"
            },
            require: {
                valFormManager: "^^valFormManager"
            }
        }
    );

    function BlockGridEntriesController($scope) {

        var vm = this;

        vm.$onInit = function () {
          console.log(vm);
        };
    }   

})();
