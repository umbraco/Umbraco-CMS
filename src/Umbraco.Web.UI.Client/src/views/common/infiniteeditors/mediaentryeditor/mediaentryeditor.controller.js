angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEntryEditorController",
        function ($scope, localizationService, entityResource, overlayService, eventsService, mediaHelper) {

            var unsubscribe = [];
            var vm = this;

            vm.loading = true;
            vm.model = $scope.model;
            vm.mediaEntry = vm.model.mediaEntry;

            localizationService.localizeMany([
                vm.model.createFlow ? "general_cancel" : "general_close",
                vm.model.createFlow ? "general_create" : "buttons_submitChanges"
            ]).then(function (data) {
                vm.closeLabel = data[0];
                vm.submitLabel = data[1];
            });

            console.log("MediaEntryEditorController", vm.model, vm.mediaEntry);

            function init() {

                updateMedia();

                unsubscribe.push(eventsService.on("editors.media.saved", function(name, args) {
                    // if this media item uses the updated media type we want to reload the media file
                    if(args && args.media && args.media.key === vm.model.mediaEntry.mediaKey) {
                        updateMedia();
                    }
                }));
            }

            function updateMedia() {

                // TODO: test that we update a media if its saved..
                console.log("updateMedia", vm.model.mediaEntry.mediaKey);

                vm.loading = true;
                entityResource.getById(vm.model.mediaEntry.mediaKey, "Media").then(function (mediaEntity) {
                    vm.media = mediaEntity;
                    vm.imageSrc = mediaHelper.resolveFileFromEntity(mediaEntity, true);
                    vm.loading = false;
                    vm.hasDimensions = false;
                    vm.isCroppable = false;
                }, function () {
                    localizationService.localize("mediaPicker_deletedItem").then(function (localized) {
                        vm.media = {
                            name: localized,
                            icon: "icon-picture",
                            trashed: true
                        };
                        vm.loading = false;
                        vm.hasDimensions = false;
                        vm.isCroppable = false;
                    });
                });
            }

            vm.onImageLoaded = onImageLoaded;
            function onImageLoaded(isCroppable, hasDimensions) {
                vm.isCroppable = isCroppable;
                vm.hasDimensions = hasDimensions;
            };


            vm.repickMedia = repickMedia;
            function repickMedia() {
                vm.model.propertyEditor.changeMediaFor(vm.model.mediaEntry, onMediaReplaced);
            }

            function onMediaReplaced() {

                // mark we have changes:
                vm.imageCropperForm.$setDirty();

                // un-select crop:
                vm.currentCrop = null;

                //
                updateMedia();
            }


            vm.focalPointChanged = function(left, top) {
                //update the model focalpoint value
                vm.mediaEntry.focalPoint = {
                    left: left,
                    top: top
                };

                //set form to dirty to track changes
                setDirty();
            }



            vm.selectCrop = selectCrop;
            function selectCrop(targetCrop) {
                vm.currentCrop = targetCrop;
                setDirty();
                // TODO: start watchin values of crop, first when changed set to dirty.
            };

            vm.deselectCrop = deselectCrop;
            function deselectCrop() {
                vm.currentCrop = null;
            };

            vm.resetCrop = resetCrop;
            function resetCrop() {
                if (vm.currentCrop) {
                    $scope.$evalAsync( () => {
                        console.log(vm.currentCrop);
                        vm.model.propertyEditor.resetCrop(vm.currentCrop);
                        vm.forceUpdateCrop = Math.random();
                        console.log(vm.forceUpdateCrop);
                    });
                }
            }

            function setDirty() {
                vm.imageCropperForm.$setDirty();
            }


            vm.submitAndClose = function () {
                if (vm.model && vm.model.submit) {
                    vm.model.submit(vm.model);
                }
            }

            vm.close = function () {
                if (vm.model && vm.model.close) {
                    if (vm.model.createFlow === true || vm.imageCropperForm.$dirty === true) {
                        var labels = vm.model.createFlow === true ? ["media_confirmCancelMediaEntryCreationHeadline", "media_confirmCancelMediaEntryCreationMessage"] : ["prompt_discardChanges", "media_confirmCancelMediaEntryHasChanges"];
                        localizationService.localizeMany(labels).then(function (localizations) {
                            const confirm = {
                                title: localizations[0],
                                view: "default",
                                content: localizations[1],
                                submitButtonLabelKey: "general_discard",
                                submitButtonStyle: "danger",
                                closeButtonLabelKey: "prompt_stay",
                                submit: function () {
                                    overlayService.close();
                                    vm.model.close(vm.model);
                                },
                                close: function () {
                                    overlayService.close();
                                }
                            };
                            overlayService.open(confirm);
                        });
                    } else {
                        vm.model.close(vm.model);
                    }

                }
            }

            init();
            $scope.$on("$destroy", function () {
                unsubscribe.forEach(x => x());
            });

        }
    );
