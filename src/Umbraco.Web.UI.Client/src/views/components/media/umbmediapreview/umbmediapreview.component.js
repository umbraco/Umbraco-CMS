(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbMediaPreview", {
            templateUrl: "views/components/media/umbmediapreview/umb-media-preview.html",
            controller: UmbMediaPreviewController,
            controllerAs: "vm",
            bindings: {
                blockConfigModel: "<"
            }
        });

    function UmbMediaPreviewController() {

        var vm = this;





        vm.$onDestroy = function () {

        }

    }

})();
