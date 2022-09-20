(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridColumnSpanEditorOption
     * @function
     *
     * @description
     * A component for the block grid column span prevalue editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockGridColumnEditorOption", {
            templateUrl: "views/propertyeditors/blockgrid/prevalue/umb-block-grid-column-editor-option.html",
            controller: BlockGridColumnOptionController,
            controllerAs: "vm",
            bindings: {
                columnSpanOption: "<",
                column: "<",
                onClickAdd: "&",
                onClickRemove: "&"
            }
        });

    function BlockGridColumnOptionController() {

        var vm = this;

        vm.$onInit = function() {

        };

        
    }

})();
