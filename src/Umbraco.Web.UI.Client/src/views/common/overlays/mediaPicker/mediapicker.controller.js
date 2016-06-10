//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Overlays.MediaPickerController",
        function ($scope, mediaResource, umbRequestHelper, entityResource, $log, mediaHelper, eventsService, treeService, $element, $timeout, $cookies, $cookieStore, localizationService) {

            if(!$scope.model.title) {
                $scope.model.title = localizationService.localize("defaultdialogs_selectMedia");
            }

            var dialogOptions = $scope.model;

            $scope.disableFolderSelect = dialogOptions.disableFolderSelect;
            $scope.onlyImages = dialogOptions.onlyImages;
            $scope.showDetails = dialogOptions.showDetails;
            $scope.multiPicker = (dialogOptions.multiPicker && dialogOptions.multiPicker !== "0") ? true : false;
            $scope.startNodeId = dialogOptions.startNodeId ? dialogOptions.startNodeId : -1;
            $scope.cropSize = dialogOptions.cropSize;
            $scope.lastOpenedNode = $cookieStore.get("umbLastOpenedMediaNodeId");
            if($scope.onlyImages){
                $scope.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
            }
            else {
                $scope.acceptedFileTypes = !mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.disallowedUploadFiles);
            }
            $scope.maxFileSize = Umbraco.Sys.ServerVariables.umbracoSettings.maxFileSize + "KB";

            $scope.model.selectedImages = [];

            //preload selected item
            $scope.target = undefined;
            if(dialogOptions.currentTarget){
                $scope.target = dialogOptions.currentTarget;
            }

            $scope.upload = function(v){
               angular.element(".umb-file-dropzone-directive .file-select").click();
            };

            $scope.dragLeave = function(el, event){
                $scope.activeDrag = false;
            };

            $scope.dragEnter = function(el, event){
                $scope.activeDrag = true;
            };

            $scope.submitFolder = function() {

               if ($scope.newFolderName) {

                  mediaResource
                     .addFolder($scope.newFolderName, $scope.currentFolder.id)
                     .then(function(data) {

                        //we've added a new folder so lets clear the tree cache for that specific item
                        treeService.clearCache({
                           cacheKey: "__media", //this is the main media tree cache key
                           childrenOf: data.parentId //clear the children of the parent
                        });

                        $scope.gotoFolder(data);

                        $scope.showFolderInput = false;

                        $scope.newFolderName = "";

                     });

               } else {
                  $scope.showFolderInput = false;
               }

            };

            $scope.enterSubmitFolder = function(event) {
                if (event.keyCode === 13) {
                    $scope.submitFolder();
                    event.stopPropagation();
                }
            };

            $scope.gotoFolder = function(folder) {

                if(!$scope.multiPicker) {
                    deselectAllImages($scope.model.selectedImages);
                }

                if(!folder){
                    folder = {id: -1, name: "Media", icon: "icon-folder"};
                }

                if (folder.id > 0) {
                    entityResource.getAncestors(folder.id, "media")
                        .then(function(anc) {
                            // anc.splice(0,1);
                            $scope.path = _.filter(anc, function (f) {
                                return f.path.indexOf($scope.startNodeId) !== -1;
                            });
                        });
                }
                else {
                    $scope.path = [];
                }

                //mediaResource.rootMedia()
                mediaResource.getChildren(folder.id)
                   .then(function(data) {
                      $scope.searchTerm = "";
                      $scope.images = data.items ? data.items : [];

                      // set already selected images to selected
                      for (var folderImageIndex = 0; folderImageIndex < $scope.images.length; folderImageIndex++) {

                         var folderImage = $scope.images[folderImageIndex];
                         var imageIsSelected = false;

                         for (var selectedImageIndex = 0; selectedImageIndex < $scope.model.selectedImages.length; selectedImageIndex++) {
                            var selectedImage = $scope.model.selectedImages[selectedImageIndex];

                            if(folderImage.key === selectedImage.key) {
                               imageIsSelected = true;
                            }
                         }

                         if(imageIsSelected) {
                            folderImage.selected = true;
                         }
                      }

                   });

                $scope.currentFolder = folder;

                // for some reason i cannot set cookies with cookieStore
                document.cookie="umbLastOpenedMediaNodeId=" + folder.id;

            };

            $scope.clickHandler = function(image, event, index) {
                if (image.isFolder) {
                    if ($scope.disableFolderSelect) {
                        $scope.gotoFolder(image);
                    } else {
                        eventsService.emit("dialogs.mediaPicker.select", image);
                        selectImage(image);
                    }

                } else {

                    eventsService.emit("dialogs.mediaPicker.select", image);

                    if($scope.showDetails) {
                        $scope.target = image;
                        $scope.target.url = mediaHelper.resolveFile(image);
                        $scope.openDetailsDialog();
                    } else {
                        selectImage(image);
                    }

                }

            };

            $scope.clickItemName = function(item) {
                if(item.isFolder) {
                    $scope.gotoFolder(item);
                }
            };

            function selectImage(image) {

                if(image.selected) {

                    for(var i = 0; $scope.model.selectedImages.length > i; i++) {

                        var imageInSelection = $scope.model.selectedImages[i];
                        if(image.key === imageInSelection.key) {
                            image.selected = false;
                            $scope.model.selectedImages.splice(i, 1);
                        }
                    }

                } else {

                    if(!$scope.multiPicker) {
                        deselectAllImages($scope.model.selectedImages);
                    }

                    image.selected = true;
                    $scope.model.selectedImages.push(image);
                }

            }

            function deselectAllImages(images) {
                for (var i = 0; i < images.length; i++) {
                    var image = images[i];
                    image.selected = false;
                }
                images.length = 0;
            }

            $scope.onUploadComplete = function () {
                $scope.gotoFolder($scope.currentFolder);
            };

            $scope.onFilesQueue = function(){
                $scope.activeDrag = false;
            };

            //default root item
            if (!$scope.target) {

               if($scope.lastOpenedNode && $scope.lastOpenedNode !== -1) {

                  entityResource.getById($scope.lastOpenedNode, "media")
                     .then(function(node){

                        // make sure that las opened node is on the same path as start node
                        var nodePath = node.path.split(",");

                        if(nodePath.indexOf($scope.startNodeId.toString()) !== -1) {
                           $scope.gotoFolder({id: $scope.lastOpenedNode, name: "Media", icon: "icon-folder"});
                        } else {
                           $scope.gotoFolder({id: $scope.startNodeId, name: "Media", icon: "icon-folder"});
                        }

                    }, function (err) {
                        $scope.gotoFolder({id: $scope.startNodeId, name: "Media", icon: "icon-folder"});
                    });

               } else {

                  $scope.gotoFolder({id: $scope.startNodeId, name: "Media", icon: "icon-folder"});

               }

            }

            $scope.openDetailsDialog = function() {

               $scope.mediaPickerDetailsOverlay = {};
               $scope.mediaPickerDetailsOverlay.show = true;

               $scope.mediaPickerDetailsOverlay.submit = function(model) {

                  $scope.model.selectedImages.push($scope.target);
                  $scope.model.submit($scope.model);

                  $scope.mediaPickerDetailsOverlay.show = false;
                  $scope.mediaPickerDetailsOverlay = null;

               };

               $scope.mediaPickerDetailsOverlay.close = function(oldModel) {
                  $scope.mediaPickerDetailsOverlay.show = false;
                  $scope.mediaPickerDetailsOverlay = null;
               };

            };


        });
