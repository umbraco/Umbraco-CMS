(function () {
    "use strict";

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

    function BlockCardController(umbRequestHelper) {

        var vm = this;

        vm.$onInit = function() {
            if(vm.blockConfigModel.thumbnail != null && vm.blockConfigModel.thumbnail != "") {
                vm.thumbnail = convertVirtualToAbsolutePath(vm.blockConfigModel.thumbnail);
            }
        }

    }

})();
