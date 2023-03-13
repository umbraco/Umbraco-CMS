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

    function BlockGridColumnOptionController(localizationService) {

      var vm = this;

      localizationService.localize("general_remove").then(function (value) {
        vm.removeLabel = value;
      })

        vm.$onInit = function() {

        };

        
    }

})();
