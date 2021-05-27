(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbMediaPreview", {
            template: "<div ng-include='vm.previewView'></div>",
            controller: UmbMediaPreviewController,
            controllerAs: "vm",
            bindings: {
                extension: "<",
                source: "<",
                name: "<",
                clientSide: "<"
            }
        });

    function UmbMediaPreviewController(mediaPreview) {

        var vm = this;


        vm.$onInit = function() {
            vm.previewView = mediaPreview.getMediaPreview(vm.extension);
            console.log("vm.previewView:", vm.previewView);
        }


        vm.$onDestroy = function () {

        }

    }

})();
