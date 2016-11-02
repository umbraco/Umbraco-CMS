//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function ($scope, mediaResource, umbRequestHelper, entityResource, $log, mediaHelper, eventsService, treeService, $cookies, $element, $timeout) {

            var dialogOptions = $scope.dialogOptions;

            $scope.onlyImages = dialogOptions.onlyImages;
            $scope.showDetails = dialogOptions.showDetails;
            $scope.multiPicker = (dialogOptions.multiPicker && dialogOptions.multiPicker !== "0") ? true : false;
            $scope.startNodeId = dialogOptions.startNodeId ? dialogOptions.startNodeId : -1;
            $scope.cropSize = dialogOptions.cropSize;


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

            $scope.submitFolder = function(e) {
                if (e.keyCode === 13) {
                    e.preventDefault();
                    
                    mediaResource
                        .addFolder($scope.newFolderName, $scope.currentFolder.id)
                        .then(function(data) {
                            $scope.showFolderInput = false;
                            $scope.newFolderName = "";

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
                    });

                $scope.currentFolder = folder;      
            };
            
          
            $scope.clickHandler = function(image, ev, select) {
                ev.preventDefault();
                
                if (image.isFolder && !select) {
                    $scope.gotoFolder(image);
                }else{
                    eventsService.emit("dialogs.mediaPicker.select", image);
                    
                    //we have 3 options add to collection (if multi) show details, or submit it right back to the callback
                    if ($scope.multiPicker) {
                        $scope.select(image);
                        image.cssclass = ($scope.dialogData.selection.indexOf(image) > -1) ? "selected" : "";
                    }else if($scope.showDetails) {
                        $scope.target= image;
                        $scope.target.url = mediaHelper.resolveFile(image);
                    }else{
                        $scope.submit(image);
                    }
                }
            };

            $scope.exitDetails = function(){
                if(!$scope.currentFolder){
                    $scope.gotoFolder();
                }

                $scope.target = undefined;
            };

            $scope.onUploadComplete = function () {
                $scope.gotoFolder($scope.currentFolder);
            };

            $scope.onFilesQueue = function(){
                $scope.activeDrag = false;
            };

            //default root item
            if(!$scope.target){
                $scope.gotoFolder({ id: $scope.startNodeId, name: "Media", icon: "icon-folder" });  
            }
        });