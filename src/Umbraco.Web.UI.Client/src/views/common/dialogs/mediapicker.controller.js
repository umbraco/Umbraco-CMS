//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function ($scope, mediaResource, $log, imageHelper) {

            $scope.options = {
                url: "/"
            };

            $log.log("added");

            $scope.$on('fileuploadadd', function(event, files){
                $scope.submitFiles();
            });    

            
            mediaResource.rootMedia()
                .then(function(data) {
                    $scope.images = data;
                    //update the thumbnail property
                    _.each($scope.images, function(img) {
                        img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                    });
                });
            

            $scope.selectMediaItem = function(image) {
                if (image.contentTypeAlias.toLowerCase() == 'folder') {
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