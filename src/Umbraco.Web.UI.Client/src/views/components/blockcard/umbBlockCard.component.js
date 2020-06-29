(function () {
    "use strict";

    // TODO: Does this belong in the property editors folder?

    angular
        .module("umbraco")
        .component("umbBlockCard", {
            templateUrl: "views/components/blockcard/umb-block-card.html",
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
