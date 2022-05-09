(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridEntry
     * @description
     * renders each row for the block grid editor
     */
    
    angular
        .module("umbraco")
        .component("umbBlockGridEntry", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-entry.html',
            controller: BlockGridEntryController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                layoutEntry: "<",
                index: "<"
            },
            require: {
                valFormManager: "^^valFormManager"
            }
        }
    );

    function BlockGridEntryController($scope) {

    }   

})();
