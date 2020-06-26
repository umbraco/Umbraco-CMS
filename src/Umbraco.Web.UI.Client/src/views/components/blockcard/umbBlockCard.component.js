(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbBlockCard", {
            templateUrl: "views/propertyeditors/blockeditor/blockcard/umb-block-card.component.html",
            controller: BlockCardController,
            controllerAs: "vm",
            transclude: true,
            bindings: {
                blockConfigModel: "<",
                elementTypeModel: "<"
            }
        });

    function BlockCardController() {
        
        var vm = this;

    }

})();
