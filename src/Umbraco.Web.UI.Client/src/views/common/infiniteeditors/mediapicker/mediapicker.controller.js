//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Editors.MediaPickerController",
        function ($scope, $timeout, mediaResource, entityResource, userService, mediaHelper, mediaTypeHelper, eventsService, treeService, localStorageService, localizationService, editorService, umbSessionStorage, notificationsService, clipboardService) {

            var vm = this;

            vm.submit = submit;
            vm.close = close;

            vm.toggle = toggle;
            vm.upload = upload;
            vm.dragLeave = dragLeave;
            vm.dragEnter = dragEnter;
            vm.onUploadComplete = onUploadComplete;
            vm.onFilesQueue = onFilesQueue;
            vm.changeSearch = changeSearch;
            vm.submitFolder = submitFolder;
            vm.enterSubmitFolder = enterSubmitFolder;
            vm.focalPointChanged = focalPointChanged;
            vm.changePagination = changePagination;
            vm.onNavigationChanged = onNavigationChanged;
            vm.clickClearClipboard = clickClearClipboard;

            vm.clickHandler = clickHandler;
            vm.clickItemName = clickItemName;
            vm.gotoFolder = gotoFolder;
            vm.toggleListView = toggleListView;
            vm.selectLayout = selectLayout;
            vm.showMediaList = false;

            vm.navigation = [];

            var dialogOptions = $scope.model;
            vm.clipboardItems = dialogOptions.clipboardItems;

            $scope.disableFolderSelect = (dialogOptions.disableFolderSelect && dialogOptions.disableFolderSelect !== "0") ? true : false;
            $scope.disableFocalPoint = (dialogOptions.disableFocalPoint && dialogOptions.disableFocalPoint !== "0") ? true : false;
            $scope.onlyImages = (dialogOptions.onlyImages && dialogOptions.onlyImages !== "0") ? true : false;
            $scope.onlyFolders = (dialogOptions.onlyFolders && dialogOptions.onlyFolders !== "0") ? true : false;
            $scope.showDetails = (dialogOptions.showDetails && dialogOptions.showDetails !== "0") ? true : false;
            $scope.multiPicker = (dialogOptions.multiPicker && dialogOptions.multiPicker !== "0") ? true : false;
            $scope.startNodeId = dialogOptions.startNodeId ? dialogOptions.startNodeId : -1;
            $scope.cropSize = dialogOptions.cropSize;
            $scope.lastOpenedNode = localStorageService.get("umbLastOpenedMediaNodeId");
            $scope.lockedFolder = true;
            $scope.allowMediaEdit = dialogOptions.allowMediaEdit ? dialogOptions.allowMediaEdit : false;

            $scope.filterOptions = {
                excludeSubFolders: umbSessionStorage.get("mediaPickerExcludeSubFolders") || false
            };

            var userStartNodes = [];

            var umbracoSettings = Umbraco.Sys.ServerVariables.umbracoSettings;
            var allowedUploadFiles = mediaHelper.formatFileTypes(umbracoSettings.allowedUploadFiles);

            if ($scope.onlyImages) {
                vm.acceptedFileTypes = mediaHelper.formatFileTypes(umbracoSettings.imageFileTypes);
            } else {
                // Use list of allowed file types if provided
                if (allowedUploadFiles !== '') {
                    vm.acceptedFileTypes = allowedUploadFiles;
                } else {
                    // If no allowed list, we pass in a disallowed list by adding ! to the file extensions, allowing everything EXCEPT for disallowedUploadFiles
                    vm.acceptedFileTypes = !mediaHelper.formatFileTypes(umbracoSettings.disallowedUploadFiles);
                }
            }

            vm.maxFileSize = umbracoSettings.maxFileSize + "KB";

            $scope.model.selection = [];

            vm.acceptedMediatypes = [];
            mediaTypeHelper.getAllowedImagetypes($scope.startNodeId)
                .then(function (types) {
                    vm.acceptedMediatypes = types;
                });

            var dataTypeKey = null;
            if ($scope.model && $scope.model.dataTypeKey) {
                dataTypeKey = $scope.model.dataTypeKey;
            }

            vm.searchOptions = {
                pageNumber: 1,
                pageSize: 100,
                totalItems: 0,
                totalPages: 0,
                filter: '',
                dataTypeKey: dataTypeKey
            };
            vm.layout = {
                layouts: [{ name: "Grid", icon: "icon-thumbnails-small", path: "gridpath", selected: true },
                { name: "List", icon: "icon-list", path: "listpath", selected: true }],
                activeLayout: { name: "Grid", icon: "icon-thumbnails-small", path: "gridpath", selected: true }
            };

            // preload selected item
            $scope.target = null;

            if (dialogOptions.currentTarget) {
                $scope.target = dialogOptions.currentTarget;
            }

          function setTitle(data) {
            if (!$scope.model.title)
              $scope.model.title = data[0];        
          }

          function setNavigation(data) {
            if (!vm.navigation.length) { 
            vm.navigation = [{
              "alias": "empty",
              "name": data[0],
              "icon": "icon-umb-media",
              "active": true,
              "view": ""
            }];
            if (vm.clipboardItems) {
              vm.navigation.push({
                "alias": "clipboard",
                "name": data[1],
                "icon": "icon-paste-in",
                "view": "",
                "disabled": vm.clipboardItems.length === 0
              });
            }
              vm.activeTab = vm.navigation[0];
            }       
          }


          function onInit() {


              localizationService.localizeMany(["defaultdialogs_selectMedia", "mediaPicker_tabClipboard"])
                .then(function (localizationResult) {
                  setTitle(localizationResult);
                  setNavigation(localizationResult);
                });
               

                userService.getCurrentUser().then(function (userData) {
                    userStartNodes = userData.startMediaIds;

                    if ($scope.startNodeId !== -1) {
                        entityResource.getById($scope.startNodeId, "media")
                            .then(function (ent) {
                                $scope.startNodeId = ent.id;
                                run();
                            });
                    } else {
                        run();
                    }
                });
            }

            function run() {
                //default root item
                if (!$scope.target) {
                    if ($scope.lastOpenedNode && $scope.lastOpenedNode !== -1) {
                        entityResource.getById($scope.lastOpenedNode, "media")
                            .then(ensureWithinStartNode, gotoStartNode);
                    } else {
                        gotoStartNode();
                    }
                } else {
                    // if a target is specified, go look it up - generally this target will just contain ids not the actual full
                    // media object so we need to look it up
                    var originalTarget = $scope.target;
                    var id = $scope.target.udi ? $scope.target.udi : $scope.target.id;
                    var altText = $scope.target.altText;
                    var caption = $scope.target.caption;

                    // ID of a UDI or legacy int ID still could be null/undefinied here
                    // As user may dragged in an image that has not been saved to media section yet
                    if (id) {
                        entityResource.getById(id, "Media")
                            .then(function (node) {
                                $scope.target = node;
                                // Moving directly to existing node's folder
                                gotoFolder({ id: node.parentId }).then(function () {
                                    selectMedia(node);
                                    $scope.target.url = mediaHelper.resolveFileFromEntity(node);
                                    $scope.target.thumbnail = mediaHelper.resolveFileFromEntity(node, true);
                                    $scope.target.altText = altText;
                                    $scope.target.caption = caption;
                                    $scope.target.focalPoint = originalTarget.focalPoint;
                                    $scope.target.coordinates = originalTarget.coordinates;
                                    openDetailsDialog();
                                });
                            }, gotoStartNode);
                    } else {
                        // No ID set - then this is going to be a tmpimg that has not been uploaded
                        // User editing this will want to be changing the ALT text
                        openDetailsDialog();
                    }
                }
            }

            function upload(v) {
                var fileSelect = $(".umb-file-dropzone .file-select");
                if (fileSelect.length === 0) {
                    localizationService.localize('media_uploadNotAllowed').then(function (message) { notificationsService.warning(message); });
                }
                else {
                    fileSelect.trigger("click");
                }
            }

            function dragLeave() {
                $scope.activeDrag = false;
            }

            function dragEnter() {
                $scope.activeDrag = true;
            }

            function submitFolder() {
                if ($scope.model.creatingFolder) {
                    return;
                }
                if ($scope.model.newFolderName) {
                    $scope.model.creatingFolder = true;
                    mediaResource
                        .addFolder($scope.model.newFolderName, $scope.currentFolder.id)
                        .then(function (data) {
                            //we've added a new folder so lets clear the tree cache for that specific item
                            treeService.clearCache({
                                cacheKey: "__media", //this is the main media tree cache key
                                childrenOf: data.parentId //clear the children of the parent
                            });
                            $scope.model.creatingFolder = false;
                            gotoFolder(data);
                            $scope.model.showFolderInput = false;
                            $scope.model.newFolderName = "";
                        });
                } else {
                    $scope.model.showFolderInput = false;
                }
            }

            function enterSubmitFolder(event) {
                if (event.keyCode === 13) {
                    submitFolder();
                    event.stopPropagation();
                }
            }

            function gotoFolder(folder) {
                if (!$scope.multiPicker) {
                    deselectAllMedia($scope.model.selection);
                }

                if (!folder) {
                    folder = { id: -1, name: "Media", icon: "icon-folder" };
                }

                if (folder.id > 0) {
                    entityResource.getAncestors(folder.id, "media", null, { dataTypeKey: dataTypeKey })
                        .then(function (anc) {
                            $scope.path = _.filter(anc,
                                function (f) {
                                    return f.path.indexOf($scope.startNodeId) !== -1;
                                });
                        });
                } else {
                    $scope.path = [];
                }

                mediaTypeHelper.getAllowedImagetypes(folder.id).then(function (types) { vm.acceptedMediatypes = types; });
                $scope.lockedFolder = (folder.id === -1 && $scope.model.startNodeIsVirtual) || hasFolderAccess(folder) === false;
                $scope.currentFolder = folder;

                localStorageService.set("umbLastOpenedMediaNodeId", folder.id);

                return getChildren(folder.id);
            }

            function toggleListView() {
                vm.showMediaList = !vm.showMediaList;
            }

            function selectLayout(layout) {
                //this somehow doesn't set the 'active=true' property for the chosen layout
                vm.layout.activeLayout = layout;
                //workaround
                vm.layout.layouts.forEach(element => element.active = false);
                layout.active = true;
                //set whether to toggle the list
                vm.showMediaList = (layout.name === "List");
            }

            function clickHandler(media, event, index) {

                if (media.isFolder) {
                    if ($scope.disableFolderSelect) {
                        gotoFolder(media);
                    } else {
                        selectMedia(media);
                    }
                } else {
                    if ($scope.showDetails) {

                        $scope.target = media;

                        // handle both entity and full media object
                        if (media.image) {
                            $scope.target.url = media.image;
                        } else {
                            $scope.target.url = mediaHelper.resolveFile(media);
                        }

                        openDetailsDialog();
                    } else {
                        selectMedia(media);
                    }
                }
            }

            function clickItemName(item, event, index) {
                if (item.isFolder) {
                    gotoFolder(item);
                }
                else {
                    clickHandler(item, event, index);
                }
            };

            function selectMedia(media) {
                if (!media.selectable) {
                    return;
                }
                if (media.selected) {
                    for (var i = 0; $scope.model.selection.length > i; i++) {
                        var imageInSelection = $scope.model.selection[i];
                        if (media.key === imageInSelection.key) {
                            media.selected = false;
                            $scope.model.selection.splice(i, 1);
                        }
                    }
                } else {
                    if (!$scope.multiPicker) {
                        deselectAllMedia($scope.model.selection);
                    }
                    eventsService.emit("dialogs.mediaPicker.select", media);
                    media.selected = true;
                    $scope.model.selection.push(media);
                }
            }

            function deselectAllMedia(medias) {
                for (var i = 0; i < medias.length; i++) {
                    var media = medias[i];
                    media.selected = false;
                }
                medias.length = 0;
            }

            function onUploadComplete(files) {
                gotoFolder($scope.currentFolder).then(function () {
                    $timeout(function () {
                        if ($scope.multiPicker) {
                            var images = _.rest(_.sortBy($scope.images, 'id'), $scope.images.length - files.length);
                            images.forEach(image => selectMedia(image));
                        } else {
                            var image = _.sortBy($scope.images, 'id')[$scope.images.length - 1];
                            clickHandler(image);
                        }
                    });
                });
            }

            function onFilesQueue() {
                $scope.activeDrag = false;
            }

            function ensureWithinStartNode(node) {
                // make sure that last opened node is on the same path as start node
                var nodePath = node.path.split(",");

                // also make sure the node is not trashed
                if (nodePath.indexOf($scope.startNodeId.toString()) !== -1 && node.trashed === false) {
                    gotoFolder({ id: $scope.lastOpenedNode || $scope.startNodeId, name: "Media", icon: "icon-folder", path: node.path });
                    return true;
                } else {
                    gotoFolder({ id: $scope.startNodeId, name: "Media", icon: "icon-folder" });
                    return false;
                }
            }

            function hasFolderAccess(node) {
                var nodePath = node.path ? node.path.split(',') : [node.id];

                for (var i = 0; i < nodePath.length; i++) {
                    if (userStartNodes.indexOf(parseInt(nodePath[i])) !== -1)
                        return true;
                }

                return false;
            }

            function gotoStartNode() {
                gotoFolder({ id: $scope.startNodeId, name: "Media", icon: "icon-folder" });
            }

            function openDetailsDialog() {

                const dialog = {
                    size: "small",
                    cropSize: $scope.cropSize,
                    target: $scope.target,
                    disableFocalPoint: $scope.disableFocalPoint,
                    submit: function () {

                        $scope.model.selection.push($scope.target);
                        $scope.model.submit($scope.model);

                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };

                localizationService.localize("defaultdialogs_editSelectedMedia").then(value => {
                    dialog.title = value;
                    editorService.mediaCropDetails(dialog);
                });
            };

            function onNavigationChanged(tab) {
              vm.activeTab.active = false;
              vm.activeTab = tab;
              vm.activeTab.active = true;
            };

            function clickClearClipboard() {
                vm.onNavigationChanged(vm.navigation[0]);
                vm.navigation[1].disabled = true;
                vm.clipboardItems = [];
                dialogOptions.clickClearClipboard();
            };

            var debounceSearchMedia = _.debounce(function () {
                $scope.$apply(function () {
                    if (vm.searchOptions.filter) {
                        searchMedia();
                    } else {

                        // reset pagination
                        vm.searchOptions = {
                            pageNumber: 1,
                            pageSize: 100,
                            totalItems: 0,
                            totalPages: 0,
                            filter: '',
                            dataTypeKey: dataTypeKey
                        };

                        getChildren($scope.currentFolder.id);
                    }
                });
            }, 500);

            function changeSearch() {
                vm.loading = true;
                debounceSearchMedia();
            }

            function toggle() {
                umbSessionStorage.set("mediaPickerExcludeSubFolders", $scope.filterOptions.excludeSubFolders);
                // Make sure to activate the changeSearch function everytime the toggle is clicked
                changeSearch();
            }

            function changePagination(pageNumber) {
                vm.loading = true;
                vm.searchOptions.pageNumber = pageNumber;
                searchMedia();
            };

            function searchMedia() {
                vm.loading = true;
                entityResource.getPagedDescendants($scope.filterOptions.excludeSubFolders ? $scope.currentFolder.id : $scope.startNodeId, "Media", vm.searchOptions)
                    .then(function (data) {

                        // update image data to work with image grid
                        if (data.items) {
                            var allowedTypes = dialogOptions.filter ? dialogOptions.filter.split(",") : null;

                            data.items.forEach(function(mediaItem) {
                                setMediaMetaData(mediaItem);
                                mediaItem.filtered = allowedTypes && allowedTypes.indexOf(mediaItem.metaData.ContentTypeAlias) < 0;
                            });
                        }

                        // update images
                        $scope.images = data.items ? data.items : [];

                        // update pagination
                        if (data.pageNumber > 0)
                            vm.searchOptions.pageNumber = data.pageNumber;
                        if (data.pageSize > 0)
                            vm.searchOptions.pageSize = data.pageSize;

                        vm.searchOptions.totalItems = data.totalItems;
                        vm.searchOptions.totalPages = data.totalPages;

                        // set already selected medias to selected
                        preSelectMedia();
                        vm.loading = false;
                    });
            }

            function setMediaMetaData(mediaItem) {
                // set thumbnail and src
                mediaItem.thumbnail = mediaHelper.resolveFileFromEntity(mediaItem, true);
                mediaItem.image = mediaHelper.resolveFileFromEntity(mediaItem, false);
                // set properties to match a media object
                if (mediaItem.metaData) {
                    mediaItem.properties = [];
                    if (mediaItem.metaData.umbracoWidth && mediaItem.metaData.umbracoHeight) {
                        mediaItem.properties.push(
                            {
                                alias: "umbracoWidth",
                                editor: mediaItem.metaData.umbracoWidth.PropertyEditorAlias,
                                value: mediaItem.metaData.umbracoWidth.Value
                            },
                            {
                                alias: "umbracoHeight",
                                editor: mediaItem.metaData.umbracoHeight.PropertyEditorAlias,
                                value: mediaItem.metaData.umbracoHeight.Value
                            }
                        );
                    }
                    if (mediaItem.metaData.umbracoFile) {
                        // this is required for resolving files through the mediahelper
                        mediaItem.properties.push(
                            {
                                alias: "umbracoFile",
                                editor: mediaItem.metaData.umbracoFile.PropertyEditorAlias,
                                value: mediaItem.metaData.umbracoFile.Value
                            }
                        );
                    }
                    if (mediaItem.metaData.UpdateDate !== null) {
                        mediaItem.updateDate = mediaItem.metaData.UpdateDate;
                    }
                }
            }

            function getChildren(id) {
                vm.loading = true;
                return entityResource.getChildren(id, "Media", vm.searchOptions).then(function (data) {

                    var allowedTypes = dialogOptions.filter ? dialogOptions.filter.split(",") : null;

                    for (var i = 0; i < data.length; i++) {
                        setDefaultData(data[i]);
                        data[i].filtered = allowedTypes && allowedTypes.indexOf(data[i].metaData.ContentTypeAlias) < 0;
                    }

                    vm.searchOptions.filter = "";
                    $scope.images = data ? data : [];

                    // set already selected medias to selected
                    preSelectMedia();
                    vm.loading = false;
                });
            }

            function setDefaultData(item) {
                if (item.metaData.MediaPath !== null) {
                    item.thumbnail = mediaHelper.resolveFileFromEntity(item, true);
                    item.image = mediaHelper.resolveFileFromEntity(item, false);
                }
                if (item.metaData.UpdateDate !== null) {
                    item.updateDate = item.metaData.UpdateDate;
                }
            }

            function preSelectMedia() {
                for (var folderIndex = 0; folderIndex < $scope.images.length; folderIndex++) {
                    var folderImage = $scope.images[folderIndex];
                    var imageIsSelected = false;

                    if ($scope.model && Utilities.isArray($scope.model.selection)) {
                        for (var selectedIndex = 0;
                            selectedIndex < $scope.model.selection.length;
                            selectedIndex++) {
                            var selectedImage = $scope.model.selection[selectedIndex];

                            if (folderImage.key === selectedImage.key) {
                                imageIsSelected = true;
                            }
                        }
                    }

                    if (imageIsSelected) {
                        folderImage.selected = true;
                    }
                }
            }

            /**
             * Called when the umbImageGravity component updates the focal point value
             * @param {any} left
             * @param {any} top
             */
            function focalPointChanged(left, top) {
                // update the model focalpoint value
                $scope.target.focalPoint = {
                    left: left,
                    top: top
                };
            }

            function submit() {
                if ($scope.model && $scope.model.submit) {
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if ($scope.model && $scope.model.close) {
                    $scope.model.close($scope.model);
                }
            }

            onInit();

        });
