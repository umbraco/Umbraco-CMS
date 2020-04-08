(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbBlockCard", {
            templateUrl: "views/propertyeditors/blockeditor/blockcard/blockcard.component.html",
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
