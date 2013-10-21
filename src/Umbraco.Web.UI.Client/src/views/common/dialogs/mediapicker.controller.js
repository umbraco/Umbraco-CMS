//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function ($scope, mediaResource, umbRequestHelper, entityResource, $log, imageHelper, eventsService) {

            var dialogOptions = $scope.$parent.dialogOptions;
            $scope.options = {
                url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
                autoUpload: true,
                formData:{
                    currentFolder: -1
                }
            };

            $scope.submitFolder = function(e){
                if(e.keyCode === 13){
                    $scope.showFolderInput = false;

                    mediaResource
                    .addFolder($scope.newFolderName, $scope.options.formData.currentFolder)
                    .then(function(data){
                        
                        $scope.gotoFolder(data.id);
                    });
                }
            };

            $scope.gotoFolder = function(folderId){

                if(folderId > 0){
                    entityResource.getAncestors(folderId, "media")
                        .then(function(anc){
                           // anc.splice(0,1);  
                            $scope.path = anc;
                        });
                }else{
                    $scope.path = [];
                }
                


                //mediaResource.rootMedia()
                mediaResource.getChildren(folderId)
                    .then(function(data) {
                        
                        $scope.images = [];
                        $scope.searchTerm = "";
                        $scope.images = data.items;
                        //update the thumbnail property
                        _.each($scope.images, function(img) {
                            img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                        });
                    });

                $scope.options.formData.currentFolder = folderId;
            };
               

            $scope.$on('fileuploadstop', function(event, files){
                $scope.gotoFolder($scope.options.formData.currentFolder);
            });
            
            $scope.clickHandler = function(image, ev){

                if (image.contentTypeAlias.toLowerCase() == 'folder') {      
                    $scope.options.formData.currentFolder = image.id;
                    $scope.gotoFolder(image.id);
                }else if (image.contentTypeAlias.toLowerCase() == 'image') {
                    eventsService.publish("Umbraco.Dialogs.MediaPickerController.Select", image).then(function(image){
                        if(dialogOptions && dialogOptions.multiPicker){
                            $scope.select(image);
                            image.cssclass = ($scope.dialogData.selection.indexOf(image) > -1) ? "selected" : "";
                        }else{
                            $scope.submit(image);                 
                        }
                    });
                }

                ev.preventDefault();
            };

            $scope.selectMediaItem = function(image) {
                if (image.contentTypeAlias.toLowerCase() == 'folder') {      
                    $scope.options.formData.currentFolder = image.id;
                    $scope.gotoFolder(image.id);
                }else if (image.contentTypeAlias.toLowerCase() == 'image') {

                    eventsService.publish("Umbraco.Dialogs.MediaPickerController.Select", image).then(function(image){
                        if(dialogOptions && dialogOptions.multiPicker){
                            $scope.select(image);
                        }else{
                            $scope.submit(image);                 
                        }
                    });
                }
            };

            $scope.gotoFolder(-1);
        });