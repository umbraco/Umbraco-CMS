//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function ($scope, entityResource, mediaHelper, $timeout, userService, localizationService, editorService, overlayService, clipboardService) {

        const vm = this;

        vm.labels = {};
        vm.labels.deletedItem = "";

        vm.add = add;
        vm.remove = remove;
        vm.copyItem = copyItem;
        vm.editItem = editItem;
        vm.showAdd = showAdd;

        vm.mediaItems = [];
        let selectedIds = [];

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;
        var onlyImages = $scope.model.config.onlyImages && $scope.model.config.onlyImages !== '0' ? true : false;
        var disableFolderSelect = $scope.model.config.disableFolderSelect && $scope.model.config.disableFolderSelect !== '0' ? true : false;

        $scope.allowEditMedia = false;
        $scope.allowAddMedia = false;
        $scope.allowRemoveMedia = !$scope.readonly;

        function setupViewModel() {
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

                        var mediaItem = found ||
                        {
                            name: vm.labels.deletedItem,
                            id: $scope.model.config.idType !== "udi" ? id : null,
                            udi: $scope.model.config.idType === "udi" ? id : null,
                            icon: "icon-picture",
                            thumbnail: null,
                            trashed: true
                        };

                        return mediaItem;
                    });

                    medias.forEach(media => appendMedia(media));

                    sync();
                });
            }
        }

        function appendMedia(media) {
            if (!media.extension && media.id && media.metaData) {
                media.extension = mediaHelper.getFileExtension(media.metaData.MediaPath);
            }

            // if there is no thumbnail, try getting one if the media is not a placeholder item
            if (!media.thumbnail && media.id && media.metaData) {
                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
            }

            vm.mediaItems.push(media);

            if ($scope.model.config.idType === "udi") {
                selectedIds.push(media.udi);
            } else {
                selectedIds.push(media.id);
            }
        }

        function sync() {
            $scope.model.value = selectedIds.join();
            removeAllEntriesAction.isDisabled = selectedIds.length === 0 || !$scope.allowRemoveMedia;
            copyAllEntriesAction.isDisabled = selectedIds.length === 0;
        }

        function setDirty() {
            if (vm.modelValueForm) {
                vm.modelValueForm.modelValue.$setDirty();
            }
        }

        function reloadUpdatedMediaItems(updatedMediaNodes) {
            // because the images can be edited through the media picker we need to
            // reload. We only reload the images that is already picked but has been updated.
            // We have to get the entities from the server because the media
            // can be edited without being selected
            vm.mediaItems.forEach(media => {
                if (updatedMediaNodes.indexOf(media.udi) !== -1) {
                    media.loading = true;
                    entityResource.getById(media.udi, "Media")
                        .then(function (mediaEntity) {
                            Utilities.extend(media, mediaEntity);
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
                .then(function (data) {
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
                        $scope.allowEditMedia = hasAccessToMedia && !$scope.readonly;
                        $scope.allowAddMedia = hasAccessToMedia && !$scope.readonly;

                        setupViewModel();
                    });
                });
        }

        function remove(index) {
            if (!$scope.allowRemoveMedia) return;
            
            vm.mediaItems.splice(index, 1);
            selectedIds.splice(index, 1);
            sync();
            setDirty();
        }

        function copyAllEntries() {
            if($scope.mediaItems.length > 0) {

                // gather aliases
                var aliases = $scope.mediaItems.map(mediaEntity => mediaEntity.metaData.ContentTypeAlias);

                // remove duplicate aliases
                aliases = aliases.filter((item, index) => aliases.indexOf(item) === index);

                var data = $scope.mediaItems.map(mediaEntity => { return {"mediaKey": mediaEntity.key }});

                localizationService.localize("clipboard_labelForArrayOfItems", [$scope.model.label]).then(function(localizedLabel) {
                    clipboardService.copyArray(clipboardService.TYPES.MEDIA, aliases, data, localizedLabel, "icon-thumbnail-list", $scope.model.id);
                });
            }
        }

        function copyItem(mediaItem) {

            var mediaEntry = {};
            mediaEntry.mediaKey = mediaItem.key;

            clipboardService.copy(clipboardService.TYPES.MEDIA, mediaItem.metaData.ContentTypeAlias, mediaEntry, mediaItem.name, mediaItem.icon, mediaItem.udi);
        }

        function pasteFromClipboard(pasteEntry, pasteType) {
            if (!$scope.readonly) return;

            if (pasteEntry === undefined) {
                return;
            }

            pasteEntry = clipboardService.parseContentForPaste(pasteEntry, pasteType);

            entityResource.getById(pasteEntry.mediaKey, "Media").then(function (mediaEntity) {

                if(disableFolderSelect === true && mediaEntity.metaData.ContentTypeAlias === "Folder") {
                    return;
                }

                appendMedia(mediaEntity);
                sync();
            });
        }

        function editItem(item) {
            if (!$scope.allowEditMedia) return;

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
                                vm.mediaItems.forEach(media => {
                                    if (media.id === model.mediaNode.id) {
                                        Utilities.extend(media, mediaEntity);
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
            if (!$scope.allowAddMedia) return;

            var mediaPicker = {
                startNodeId: $scope.model.config.startNodeId,
                startNodeIsVirtual: $scope.model.config.startNodeIsVirtual,
                dataTypeKey: $scope.model.dataTypeKey,
                multiPicker: multiPicker,
                onlyImages: onlyImages,
                disableFolderSelect: disableFolderSelect,
                clickPasteItem: function(item, mouseEvent) {
                    if (Array.isArray(item.data)) {
                        var indexIncrementor = 0;
                        item.data.forEach(function (entry) {
                            if (pasteFromClipboard(entry, item.type)) {
                                indexIncrementor++;
                            }
                        });
                    } else {
                        pasteFromClipboard(item.data, item.type);
                    }
                    if(!(mouseEvent.ctrlKey || mouseEvent.metaKey)) {
                        editorService.close();
                    }
                    setDirty();
                },
                submit: function (model) {

                    editorService.close();

                    model.selection.forEach(media => {
                        // if there is no thumbnail, try getting one if the media is not a placeholder item
                        if (!media.thumbnail && media.id && media.metaData) {
                            media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                        }

                        vm.mediaItems.push(media);

                        if ($scope.model.config.idType === "udi") {
                            selectedIds.push(media.udi);
                        }
                        else {
                            selectedIds.push(media.id);
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


            var allowedTypes = null;
            if(onlyImages) {
                allowedTypes = ["Image"]; // Media Type Image Alias.
            }

            mediaPicker.clickClearClipboard = function ($event) {
                clipboardService.clearEntriesOfType(clipboardService.TYPES.Media, allowedTypes);
            };

            mediaPicker.clipboardItems = clipboardService.retrieveEntriesOfType(clipboardService.TYPES.MEDIA, allowedTypes);
            mediaPicker.clipboardItems.sort( (a, b) => {
                return b.date - a.date
            });

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
            if (!$scope.allowRemoveMedia) return;

            localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[1],
                    content: data[0],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.mediaItems.length = 0;// AngularJS way to empty the array.
                        selectedIds.length = 0;// AngularJS way to empty the array.
                        sync();
                        setDirty();
                        overlayService.close();
                    }
                });
            });
        }

        let copyAllEntriesAction = {
            labelKey: "clipboard_labelForCopyAllEntries",
            labelTokens: ['Media'],
            icon: "icon-documents",
            method: copyAllEntries,
            isDisabled: true,
            useLegacyIcon: false
        }

        let removeAllEntriesAction = {
            labelKey: "clipboard_labelForRemoveAllEntries",
            labelTokens: [],
            icon: "icon-trash",
            method: removeAllEntries,
            isDisabled: true,
            useLegacyIcon: false
        };

        if (multiPicker === true) {
            var propertyActions = [
                copyAllEntriesAction,
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
            disabled: !multiPicker || $scope.readonly,
            items: "li:not(.add-wrapper)",
            cancel: ".unsortable",
            update: function () {
                setDirty();
                $timeout(function () {
                    // TODO: Instead of doing this with a timeout would be better to use a watch like we do in the
                    // content picker. Then we don't have to worry about setting ids, render models, models, we just set one and let the
                    // watch do all the rest.
                    selectedIds = vm.mediaItems.map(media => $scope.model.config.idType === "udi" ? media.udi : media.id);

                    sync();
                });
            }
        };

        init();

    });
