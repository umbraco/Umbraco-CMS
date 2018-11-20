//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function ($scope, entityResource, mediaHelper, $timeout, userService, localizationService, editorService) {

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;
        var onlyImages = $scope.model.config.onlyImages && $scope.model.config.onlyImages !== '0' ? true : false;
        var disableFolderSelect = $scope.model.config.disableFolderSelect && $scope.model.config.disableFolderSelect !== '0' ? true : false;
        $scope.allowEditMedia = false;
        $scope.allowAddMedia = false;

        function setupViewModel() {
            $scope.mediaItems = [];
            $scope.ids = [];

            $scope.isMultiPicker = multiPicker;

            if ($scope.model.value) {
                var ids = $scope.model.value.split(',');

                //NOTE: We need to use the entityResource NOT the mediaResource here because
                // the mediaResource has server side auth configured for which the user must have
                // access to the media section, if they don't they'll get auth errors. The entityResource
                // acts differently in that it allows access if the user has access to any of the apps that
                // might require it's use. Therefore we need to use the metaData property to get at the thumbnail
                // value.

                entityResource.getByIds(ids, "Media").then(function (medias) {

                    // The service only returns item results for ids that exist (deleted items are silently ignored).
                    // This results in the picked items value to be set to contain only ids of picked items that could actually be found.
                    // Since a referenced item could potentially be restored later on, instead of changing the selected values here based
                    // on whether the items exist during a save event - we should keep "placeholder" items for picked items that currently
                    // could not be fetched. This will preserve references and ensure that the state of an item does not differ depending
                    // on whether it is simply resaved or not.
                    // This is done by remapping the int/guid ids into a new array of items, where we create "Deleted item" placeholders
                    // when there is no match for a selected id. This will ensure that the values being set on save, are the same as before.

                    medias = _.map(ids,
                        function (id) {
                            var found = _.find(medias,
                                function (m) {
                                    // We could use coercion (two ='s) here .. but not sure if this works equally well in all browsers and
                                    // it's prone to someone "fixing" it at some point without knowing the effects. Rather use toString()
                                    // compares and be completely sure it works.
                                    return m.udi.toString() === id.toString() || m.id.toString() === id.toString();
                                });
                            if (found) {
                                return found;
                            } else {
                                return {
                                    name: localizationService.dictionary.mediaPicker_deletedItem,
                                    id: $scope.model.config.idType !== "udi" ? id : null,
                                    udi: $scope.model.config.idType === "udi" ? id : null,
                                    icon: "icon-picture",
                                    thumbnail: null,
                                    trashed: true
                                };
                            }
                        });

                    _.each(medias,
                        function (media, i) {
                            // if there is no thumbnail, try getting one if the media is not a placeholder item
                            if (!media.thumbnail && media.id && media.metaData) {
                                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                            }

                            $scope.mediaItems.push(media);

                            if ($scope.model.config.idType === "udi") {
                                $scope.ids.push(media.udi);
                            } else {
                                $scope.ids.push(media.id);
                            }
                        });

                    sync();
                });
            }
        }

        function sync() {
            $scope.model.value = $scope.ids.join();
        };

        function reloadUpdatedMediaItems(updatedMediaNodes) {
            // because the images can be edited through the media picker we need to 
            // reload. We only reload the images that is already picked but has been updated.
            // We have to get the entities from the server because the media 
            // can be edited without being selected
            _.each($scope.images, function (image, i) {
                if (updatedMediaNodes.indexOf(image.udi) !== -1) {
                    image.loading = true;
                    entityResource.getById(image.udi, "media")
                        .then(function (mediaEntity) {
                            angular.extend(image, mediaEntity);
                            image.thumbnail = mediaHelper.resolveFileFromEntity(image, true);
                            image.loading = false;
                        });
                }
            })
        }

        function init() {

            userService.getCurrentUser().then(function (userData) {
                if (!$scope.model.config.startNodeId) {
                    $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                    $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                }
                // only allow users to add and edit media if they have access to the media section
                var hasAccessToMedia = userData.allowedSections.indexOf("media") !== -1;
                $scope.allowEditMedia = hasAccessToMedia;
                $scope.allowAddMedia = hasAccessToMedia;

                setupViewModel();

                //When the model value changes sync the view model
                $scope.$watch("model.value",
                    function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            setupViewModel();
                        }
                    });
            });

        }

        $scope.remove = function (index) {
            $scope.mediaItems.splice(index, 1);
            $scope.ids.splice(index, 1);
            sync();
        };

        $scope.editItem = function (item) {
            var mediaEditor = {
                id: item.id,
                submit: function (model) {
                    editorService.close();
                    // update the selected media item to match the saved media item
                    // the media picker is using media entities so we get the
                    // entity so we easily can format it for use in the media grid
                    if (model && model.mediaNode) {
                        entityResource.getById(model.mediaNode.id, "media")
                            .then(function (mediaEntity) {
                                // if an image is selecting more than once 
                                // we need to update all the media items
                                angular.forEach($scope.images, function (image) {
                                    if (image.id === model.mediaNode.id) {
                                        angular.extend(image, mediaEntity);
                                        image.thumbnail = mediaHelper.resolveFileFromEntity(image, true);
                                    }
                                });
                            });
                    }
                },
                close: function (model) {
                    editorService.close();
                }
            };
            editorService.mediaEditor(mediaEditor);
        };

        $scope.add = function () {
            var mediaPicker = {
                startNodeId: $scope.model.config.startNodeId,
                startNodeIsVirtual: $scope.model.config.startNodeIsVirtual,
                multiPicker: multiPicker,
                onlyImages: onlyImages,
                disableFolderSelect: disableFolderSelect,

                allowMediaEdit: true,
                submit: function(model) {

                    editorService.close();

                    _.each(model.selectedImages, function (media, i) {
                        // if there is no thumbnail, try getting one if the media is not a placeholder item
                        if (!media.thumbnail && media.id && media.metaData) {
                            media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                        }

                       $scope.mediaItems.push(media);

                        if ($scope.model.config.idType === "udi") {
                            $scope.ids.push(media.udi);
                        }
                        else {
                            $scope.ids.push(media.id);
                        }

                    });
                    sync();
                    reloadUpdatedMediaItems(model.updatedMediaNodes);
                },
                close: function (model) {
                    editorService.close();
                    reloadUpdatedMediaItems(model.updatedMediaNodes);
                }
            }

            editorService.mediaPicker(mediaPicker);

        };

        

        $scope.sortableOptions = {
            disabled: !$scope.isMultiPicker,
            items: "li:not(.add-wrapper)",
            cancel: ".unsortable",
            update: function (e, ui) {
                var r = [];
                // TODO: Instead of doing this with a half second delay would be better to use a watch like we do in the
                // content picker. Then we don't have to worry about setting ids, render models, models, we just set one and let the
                // watch do all the rest.
                $timeout(function () {
                    angular.forEach($scope.mediaItems, function(value, key) {
                        r.push($scope.model.config.idType === "udi" ? value.udi : value.id);
                    });
                    $scope.ids = r;
                    sync();
                }, 500, false);
            }
        };

        $scope.showAdd = function () {
            if (!multiPicker) {
                if ($scope.model.value && $scope.model.value !== "") {
                    return false;
                }
            }
            return true;
        };

        init();
        
    });
