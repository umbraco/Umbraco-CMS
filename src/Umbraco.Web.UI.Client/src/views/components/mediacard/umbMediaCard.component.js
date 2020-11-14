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
                mediaKey: "<",
                onNameClicked: "&?"
            }
        });

    function MediaCardController($scope, entityResource, mediaHelper, eventsService, localizationService) {

        var unsubscribe = [];
        var vm = this;
        vm.paddingBottom = 100;// Square while loading.
        vm.loading = true;

        var unwatch = $scope.$watch("vm.mediaKey", (newValue, oldValue) => {
            if(newValue !== oldValue) {
                vm.updateThumbnail();
            }
        });

        vm.$onInit = function () {

            vm.updateThumbnail();

            unsubscribe.push(eventsService.on("editors.media.saved", function(name, args) {
                // if this media item uses the updated media type we want to reload the media file
                if(args && args.media && args.media.key === vm.mediaKey) {
                    vm.updateThumbnail();
                }
            }));
        }


        vm.$onDestroy = function () {
            unwatch();
            unsubscribe.forEach(x => x());
        }

        vm.updateThumbnail = function () {

            // TODO: test that we update a media if its saved..
            console.log("updateThumbnail", vm.mediaKey)

            vm.loading = true;

            entityResource.getById(vm.mediaKey, "Media").then(function (mediaEntity) {
                vm.media = mediaEntity;
                vm.thumbnail = mediaHelper.resolveFileFromEntity(mediaEntity, true);

                vm.loading = false;
            }, function () {
                localizationService.localize("mediaPicker_deletedItem").then(function (localized) {
                    vm.media = {
                        name: localized,
                        icon: "icon-picture",
                        trashed: true
                    };
                    vm.loading = false;
                });
            });

        }

    }

})();
