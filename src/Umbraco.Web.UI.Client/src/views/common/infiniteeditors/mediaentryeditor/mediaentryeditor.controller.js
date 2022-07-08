angular.module("umbraco")
    .controller("Umbraco.Editors.MediaEntryEditorController",
        function ($scope, localizationService, entityResource, editorService, overlayService, eventsService, mediaHelper) {

            var unsubscribe = [];
            
            const vm = this;
            
            vm.loading = true;
            vm.model = $scope.model;
            vm.mediaEntry = vm.model.mediaEntry;
            vm.currentCrop = null;
            vm.title = "";
            
            vm.focalPointChanged = focalPointChanged;
            vm.onImageLoaded = onImageLoaded;
            vm.openMedia = openMedia;
            vm.repickMedia = repickMedia;
            vm.selectCrop = selectCrop;
            vm.deselectCrop = deselectCrop;
            vm.resetCrop = resetCrop;
            vm.submitAndClose = submitAndClose;
            vm.close = close;   

            function init() {

                localizationService.localizeMany([
                  vm.model.createFlow ? "general_cancel" : "general_close",
                  vm.model.createFlow ? "general_create" : "buttons_submitChanges"
                ]).then(data => {
                  vm.closeLabel = data[0];
                  vm.submitLabel = data[1];
                });

                updateMedia();

                unsubscribe.push(eventsService.on("editors.media.saved", function(name, args) {
                    // if this media item uses the updated media type we want to reload the media file
                    if(args && args.media && args.media.key === vm.mediaEntry.mediaKey) {
                        updateMedia();
                    }
                }));
            }

            function updateMedia() {

                vm.loading = true;

                entityResource.getById(vm.mediaEntry.mediaKey, "Media").then(function (mediaEntity) {
                    vm.media = mediaEntity;
                    vm.imageSrc = mediaHelper.resolveFileFromEntity(mediaEntity, true);
                    vm.fileSrc = mediaHelper.resolveFileFromEntity(mediaEntity, false);
                    vm.fileExtension = mediaHelper.getFileExtension(vm.fileSrc);
                    vm.loading = false;
                    vm.hasDimensions = false;
                    vm.isCroppable = false;

                    localizationService.localize("mediaPicker_editMediaEntryLabel", [vm.media.name, vm.model.documentName]).then(data => {
                        vm.title = data;
                    });
                }, function () {
                    localizationService.localize("mediaPicker_deletedItem").then(localized => {
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
            
            function onImageLoaded(isCroppable, hasDimensions) {
                vm.isCroppable = isCroppable;
                vm.hasDimensions = hasDimensions;
            }
            
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
            
            function openMedia() {

                const mediaEditor = {
                    id: vm.mediaEntry.mediaKey,
                    submit: () => {
                        editorService.close();
                    },
                    close: () => {
                        editorService.close();
                    }
                };
                
                editorService.mediaEditor(mediaEditor);
            }

            function focalPointChanged(left, top) {
                //update the model focalpoint value
                vm.mediaEntry.focalPoint = {
                    left: left,
                    top: top
                };

                //set form to dirty to track changes
                setDirty();
            }
            
            function selectCrop(targetCrop) {
                vm.currentCrop = targetCrop;
                setDirty();
                // TODO: start watchin values of crop, first when changed set to dirty.
            }
            
            function deselectCrop() {
                vm.currentCrop = null;
            }
            
            function resetCrop() {
                if (vm.currentCrop) {
                    $scope.$evalAsync( () => {
                        vm.model.propertyEditor.resetCrop(vm.currentCrop);
                        vm.forceUpdateCrop = Math.random();
                    });
                }
            }

            function setDirty() {
                vm.imageCropperForm.$setDirty();
            }

            function submitAndClose() {
                if (vm.model && vm.model.submit) {
                    vm.model.submit(vm.model);
                }
            }
            
            function close() {
                if (vm.model && vm.model.close)
                {
                    if (vm.model.createFlow === true || vm.imageCropperForm.$dirty === true)
                    {
                      const labelKeys = vm.model.createFlow === true
                          ? ["mediaPicker_confirmCancelMediaEntryCreationHeadline", "mediaPicker_confirmCancelMediaEntryCreationMessage"]
                          : ["prompt_discardChanges", "mediaPicker_confirmCancelMediaEntryHasChanges"];
                        
                        localizationService.localizeMany(labelKeys).then(localizations => {
                            const confirm = {
                                title: localizations[0],
                                view: "default",
                                content: localizations[1],
                                submitButtonLabelKey: "general_discard",
                                submitButtonStyle: "danger",
                                closeButtonLabelKey: "prompt_stay",
                                submit: () => {
                                    overlayService.close();
                                    vm.model.close(vm.model);
                                },
                                close: () => {
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
