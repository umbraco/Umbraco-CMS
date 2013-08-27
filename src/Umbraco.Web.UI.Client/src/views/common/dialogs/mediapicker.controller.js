//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function ($scope, mediaResource, $log, imageHelper) {

            $scope.options = {
                url: "/umbraco/UmbracoApi/MediaUpload/Post",
                formData:{
                    currentFolder: -1
                }
            };

            $scope.gotoFolder = function(folderId){
                //mediaResource.rootMedia()
                mediaResource.getChildren(folderId)
                    .then(function(data) {
                        $scope.images = data;
                        //update the thumbnail property
                        _.each($scope.images, function(img) {
                            img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                        });
                    });
            }
            

            $scope.$on('fileuploadadd', function(event, files){
                $scope.submitFiles();
            });

            $scope.$on('fileuploadstop', function(event, files){
                alert("done");
            });
            

            $scope.selectMediaItem = function(image) {
                if (image.contentTypeAlias.toLowerCase() == 'folder') {        
                    options.formData.currentFolder = image.id;
                    mediaResource.getChildren(image.id)
                        .then(function(data) {
                            $scope.images = data;
                            //update the thumbnail property
                            _.each($scope.images, function (img) {
                                img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                            });
                        });
                }
                else if (image.contentTypeAlias.toLowerCase() == 'image') {
                    $scope.select(image);
                }
            };

        });