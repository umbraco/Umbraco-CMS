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
                blockConfigModel: "<?",
                elementTypeModel: "<"
            }
        });

    function BlockCardController($scope, umbRequestHelper) {

        const vm = this;
        vm.styleBackgroundImage = "none";

        var unwatch = $scope.$watch("vm.blockConfigModel.thumbnail", (newValue, oldValue) => {
            if (newValue !== oldValue) {
                vm.updateThumbnail();
            }
        });

        vm.$onInit = function () {
            vm.updateThumbnail();
        };

        vm.$onChanges = function () {
            vm.icon = vm.elementTypeModel ? vm.elementTypeModel.icon.split(" ")[0] : 'icon-block';
        };

        vm.$onDestroy = function () {
            unwatch();
        };

        vm.updateThumbnail = function () {
            if (vm.blockConfigModel == null || vm.blockConfigModel.thumbnail == null || vm.blockConfigModel.thumbnail === "") {
                vm.styleBackgroundImage = "none";
                return;
            }

            var path = umbRequestHelper.convertVirtualToAbsolutePath(vm.blockConfigModel.thumbnail);
            if (path.toLowerCase().endsWith(".svg") === false) {
                path += "?width=400";
            }
            vm.styleBackgroundImage = `url('${path}')`;
        };

    }

})();
