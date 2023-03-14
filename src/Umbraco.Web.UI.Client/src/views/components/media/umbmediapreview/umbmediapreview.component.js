(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbMediaPreview", {
            template: "<div ng-include='vm.previewView' class='umb-media-preview'></div><umb-load-indicator ng-if='vm.loading'></umb-load-indicator>",
            controller: UmbMediaPreviewController,
            controllerAs: "vm",
            bindings: {
                extension: "<",
                source: "<",
                name: "<",
                clientSide: "<?",
                clientSideData: "<?"
            }
        });

    function UmbMediaPreviewController($scope, mediaPreview) {

        var vm = this;

        vm.loading = false;

        vm.$onInit = function() {
            vm.previewView = mediaPreview.getMediaPreview(vm.extension);
        }

        $scope.$on("mediaPreviewLoadingStart", () => {
            vm.loading = true;
        })
        $scope.$on("mediaPreviewLoadingComplete", () => {
            vm.loading = false;
        })

    }

})();
