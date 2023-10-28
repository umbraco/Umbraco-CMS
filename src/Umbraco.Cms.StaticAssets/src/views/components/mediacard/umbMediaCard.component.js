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
                mediaKey: "<?",
                icon: "<?",
                name: "<?",
                hasError: "<",
                allowedTypes: "<?",
                onNameClicked: "&?"
            }
        });

    function MediaCardController($scope, $element, entityResource, mediaHelper, eventsService, localizationService) {

        var unsubscribe = [];
        var vm = this;
        vm.paddingBottom = 100;// Square while loading.
        vm.loading = false;

        unsubscribe.push($scope.$watch("vm.mediaKey", (newValue, oldValue) => {
            if(newValue !== oldValue) {
                vm.updateThumbnail();
            }
        }));

        function checkErrorState() {

            vm.notAllowed = (vm.media &&vm.allowedTypes && vm.allowedTypes.length > 0 && vm.allowedTypes.indexOf(vm.media.metaData.ContentTypeAlias) === -1);

            if (
                vm.hasError === true || vm.notAllowed === true || (vm.media && vm.media.trashed === true)
            ) {
                $element.addClass("--hasError")
                vm.mediaCardForm.$setValidity('error', false)
            } else {
                $element.removeClass("--hasError")
                vm.mediaCardForm.$setValidity('error', true)
            }
        }

        vm.$onInit = function () {

            unsubscribe.push($scope.$watchGroup(["vm.media.trashed", "vm.hasError"], checkErrorState));

            vm.updateThumbnail();

            unsubscribe.push(eventsService.on("editors.media.saved", function(name, args) {
                // if this media item uses the updated media type we want to reload the media file
                if(args && args.media && args.media.key === vm.mediaKey) {
                    vm.updateThumbnail();
                }
            }));
        }


        vm.$onDestroy = function () {
            unsubscribe.forEach(x => x());
        }

        vm.updateThumbnail = function () {

            if(vm.mediaKey && vm.mediaKey !== "") {
                vm.loading = true;

                entityResource.getById(vm.mediaKey, "Media").then(function (mediaEntity) {
                    vm.media = mediaEntity;
                    checkErrorState();
                    vm.thumbnail = mediaHelper.resolveFileFromEntity(mediaEntity, true);
                    vm.fileExtension = mediaHelper.getFileExtension(vm.media.metaData.MediaPath);

                    vm.loading = false;
                }, function () {
                    localizationService.localize("mediaPicker_deletedItem").then(function (localized) {
                        vm.media = {
                            name: localized,
                            icon: "icon-picture",
                            trashed: true
                        };
                        vm.loading = false;
                        $element.addClass("--hasError")
                        vm.mediaCardForm.$setValidity('error', false)
                    });
                });
            }

        }

    }

})();
