//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function($scope, mediaResource, umbRequestHelper, entityResource, $log, imageHelper, eventsService, treeService) {

            var dialogOptions = $scope.$parent.dialogOptions;
            $scope.onlyImages = dialogOptions.onlyImages;

            $scope.options = {
                url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
                autoUpload: true,
                formData: {
                    currentFolder: -1
                }
            };

            $scope.submitFolder = function(e) {
                if (e.keyCode === 13) {
                    e.preventDefault();
                    $scope.showFolderInput = false;

                    mediaResource
                        .addFolder($scope.newFolderName, $scope.options.formData.currentFolder)
                        .then(function(data) {

                            //we've added a new folder so lets clear the tree cache for that specific item
                            treeService.clearCache({
                                cacheKey: "__media", //this is the main media tree cache key
                                childrenOf: data.parentId //clear the children of the parent
                            });

                            $scope.gotoFolder(data);
                        });
                }
            };

            $scope.gotoFolder = function(folder) {

                if (folder.id > 0) {
                    entityResource.getAncestors(folder.id, "media")
                        .then(function(anc) {
                            // anc.splice(0,1);  
                            $scope.path = anc;
                        });
                }
                else {
                    $scope.path = [];
                }

                //mediaResource.rootMedia()
                mediaResource.getChildren(folder.id)
                    .then(function(data) {

                        $scope.images = [];
                        $scope.searchTerm = "";
                        $scope.images = data.items;

                        //update the thumbnail property
                        _.each($scope.images, function(img) {
                            img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                        });

                        //reject all images that have an empty thumbnail - this can occur if there's an image item
                        // that doesn't have an uploaded image.

                        if($scope.onlyImages){
                            $scope.images = _.reject($scope.images, function(item) {
                                return item.contentTypeAlias.toLowerCase() !== "folder" && item.thumbnail === "";
                            });    
                        }
                    });

                $scope.options.formData.currentFolder = folder.id;
                $scope.currentFolder = folder;   

            };


            $scope.$on('fileuploadstop', function(event, files) {
                $scope.gotoFolder($scope.options.formData.currentFolder);
            });

            $scope.clickHandler = function(image, ev) {

                if (image.contentTypeAlias.toLowerCase() == 'folder') {
                    $scope.gotoFolder(image);
                }else{
                    eventsService.publish("Umbraco.Dialogs.MediaPickerController.Select", image);
                    
                    if (dialogOptions && dialogOptions.multiPicker) {
                        $scope.select(image);
                        image.cssclass = ($scope.dialogData.selection.indexOf(image) > -1) ? "selected" : "";
                    }
                    else {
                        $scope.submit(image);
                    }
                }

                ev.preventDefault();
            };

            $scope.selectFolder= function(folder) {
                if (dialogOptions && dialogOptions.multiPicker) {
                    $scope.select(folder);
                }
                else {
                    $scope.submit(folder);
                }                
            };

            $scope.selectMediaItem = function(image) {
                if (image.contentTypeAlias.toLowerCase() == 'folder') {
                    $scope.gotoFolder(image);
                }else{
                    eventsService.publish("Umbraco.Dialogs.MediaPickerController.Select", image);
                    
                    if (dialogOptions && dialogOptions.multiPicker) {
                        $scope.select(image);
                    }
                    else {
                        $scope.submit(image);
                    }
                }
            };

            //default root item
            $scope.gotoFolder({id: -1, name: "Media", icon: "icon-folder"});
        });