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

        vm.$onInit = function() {
            console.log(vm.blockConfigModel);
            console.log(vm.elementTypeModel);
        }

    }

})();
