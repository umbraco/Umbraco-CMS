(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("umbMediaCard", {
            templateUrl: "views/components/mediacard/umb-media-card.html",
            controller: MediaCardController,
            controllerAs: "vm",
            transclude: true,
            bindings: {
                mediaUdi: "<"
            }
        });

    function MediaCardController($scope, entityResource) {

        var vm = this;
        vm.loading = true;

        var unwatch = $scope.$watch("vm.mediaUdi", (newValue, oldValue) => {
            if(newValue !== oldValue) {
                vm.updateThumbnail();
            }
        });

        vm.$onInit = function () {

            vm.updateThumbnail();

        }
        vm.$onDestroy = function () {
            unwatch();
        }

        vm.updateThumbnail = function () {

            vm.loading = true;
            entityResource.getById(media.udi, "Media").then(function (mediaEntity) {
                vm.media = mediaEntity;
                vm.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                vm.loading = false;
            });
        }

        vm.clickButton = function () {

            console.log("click button!")

        }

    }

})();
