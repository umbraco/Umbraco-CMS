//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function ($scope, entityResource, mediaHelper, $timeout, userService, localizationService, editorService, angularHelper) {

        var vm = this;

        vm.labels = {};
        vm.labels.deletedItem = "";

        vm.add = add;
        vm.remove = remove;
        vm.editItem = editItem;
        vm.showAdd = showAdd;

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

                    medias = ids.map(id => {
                        // We could use coercion (two ='s) here .. but not sure if this works equally well in all browsers and
                        // it's prone to someone "fixing" it at some point without knowing the effects. Rather use toString()
                        // compares and be completely sure it works.
                        var found = medias.find(m => m.udi.toString() === id.toString() || m.id.toString() === id.toString());
                        if (found) {
                            return found;
                        } else {
                            return {
                                name: vm.labels.deletedItem,
                                id: $scope.model.config.idType !== "udi" ? id : null,
                                udi: $scope.model.config.idType === "udi" ? id : null,
                                icon: "icon-picture",
                                thumbnail: null,
                                trashed: true
                            };
                        }
                    });

                    medias.forEach(media => {
                        if (!media.extension && media.id && media.metaData) {
                            media.extension = mediaHelper.getFileExtension(media.metaData.MediaPath);
                        }

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
            removeAllEntriesAction.isDisabled = $scope.ids.length === 0;
        }

        function setDirty() {
            angularHelper.getCurrentForm($scope).$setDirty();
        }

        function reloadUpdatedMediaItems(updatedMediaNodes) {
            // because the images can be edited through the media picker we need to 
            // reload. We only reload the images that is already picked but has been updated.
            // We have to get the entities from the server because the media 
            // can be edited without being selected
            $scope.mediaItems.forEach(media => {
                if (updatedMediaNodes.indexOf(media.udi) !== -1) {
                    media.loading = true;
                    entityResource.getById(media.udi, "Media")
                        .then(function (mediaEntity) {
                            angular.extend(media, mediaEntity);
                            media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                            media.loading = false;
                        });
                }
            });
        }

        function init() {

            // localize labels
            var labelKeys = [
                "mediaPicker_deletedItem",
                "mediaPicker_trashed"
            ];

            localizationService.localizeMany(labelKeys)
                .then(function(data) {
                    vm.labels.deletedItem = data[0];
                    vm.labels.trashed = data[1];

                    userService.getCurrentUser().then(function (userData) {

                        if (!$scope.model.config.startNodeId) {
                            if ($scope.model.config.ignoreUserStartNodes === true) {
                                $scope.model.config.startNodeId = -1;
                                $scope.model.config.startNodeIsVirtual = true;
                            }
                            else {
                                $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                                $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                            }  
                        }

                        // only allow users to add and edit media if they have access to the media section
                        var hasAccessToMedia = userData.allowedSections.indexOf("media") !== -1;
                        $scope.allowEditMedia = hasAccessToMedia;
                        $scope.allowAddMedia = hasAccessToMedia;

                        setupViewModel();
                    });
                });
        }

        function remove(index) {
            $scope.mediaItems.splice(index, 1);
            $scope.ids.splice(index, 1);
            sync();
            setDirty();
        }

        function editItem(item) {
            var mediaEditor = {
                id: item.id,
                submit: function (model) {
                    editorService.close();
                    // update the selected media item to match the saved media item
                    // the media picker is using media entities so we get the
                    // entity so we easily can format it for use in the media grid
                    if (model && model.mediaNode) {
                        entityResource.getById(model.mediaNode.id, "Media")
                            .then(function (mediaEntity) {
                                // if an image is selecting more than once 
                                // we need to update all the media items
                                $scope.mediaItems.forEach(media => {
                                    if (media.id === model.mediaNode.id) {
                                        angular.extend(media, mediaEntity);
                                        media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                                    }
                                });
                            });
                    }
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.mediaEditor(mediaEditor);
        }

        function add() {
            var mediaPicker = {
                startNodeId: $scope.model.config.startNodeId,
                startNodeIsVirtual: $scope.model.config.startNodeIsVirtual,
                dataTypeKey: $scope.model.dataTypeKey,
                multiPicker: multiPicker,
                onlyImages: onlyImages,
                disableFolderSelect: disableFolderSelect,

                submit: function (model) {

                    editorService.close();

                    model.selection.forEach(media => {
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
                    setDirty();
                },
                close: function (model) {
                    editorService.close();
                    reloadUpdatedMediaItems(model.updatedMediaNodes);
                }
            }

            editorService.mediaPicker(mediaPicker);
        }

        function showAdd() {
            if (!multiPicker) {
                if ($scope.model.value && $scope.model.value !== "") {
                    return false;
                }
            }
            return true;
        }

        function removeAllEntries() {
            $scope.mediaItems.length = 0;// AngularJS way to empty the array.
            $scope.ids.length = 0;// AngularJS way to empty the array.
            sync();
            setDirty();
        }

        var removeAllEntriesAction = {
            labelKey: 'clipboard_labelForRemoveAllEntries',
            labelTokens: [],
            icon: 'trash',
            method: removeAllEntries,
            isDisabled: true
        };
        
        if (multiPicker === true) {
            var propertyActions = [
                removeAllEntriesAction
            ];

            if ($scope.umbProperty) {
                $scope.umbProperty.setPropertyActions(propertyActions);
            }
        }

        $scope.sortableOptions = {
            containment: 'parent',
            cursor: 'move',
            tolerance: 'pointer',
            disabled: !multiPicker,
            items: "li:not(.add-wrapper)",
            cancel: ".unsortable",
            update: function () {
                setDirty();
                $timeout(function() {
                    // TODO: Instead of doing this with a timeout would be better to use a watch like we do in the
                    // content picker. Then we don't have to worry about setting ids, render models, models, we just set one and let the
                    // watch do all the rest.
                    $scope.ids = $scope.mediaItems.map(media => $scope.model.config.idType === "udi" ? media.udi : media.id);
                    
                    sync();
                });
            }
        };

        init();

    });
