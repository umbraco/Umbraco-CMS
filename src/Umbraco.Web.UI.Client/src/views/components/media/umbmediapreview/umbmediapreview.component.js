(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbMediaPreview", {
            template: "<div ng-include='vm.previewView' class='umb-media-preview'></div>",
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

    function UmbMediaPreviewController(mediaPreview) {

        var vm = this;

        vm.$onInit = function() {
            vm.previewView = mediaPreview.getMediaPreview(vm.extension);
        }

    }

})();
