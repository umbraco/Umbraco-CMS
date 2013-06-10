//used for the media picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.MediaPickerController",
        function($scope, mediaResource) {

            mediaResource.rootMedia()
                .then(function(data) {
                    $scope.images = data;
                });

            $scope.selectMediaItem = function(image) {
                if (image.contentTypeAlias.toLowerCase() == 'folder') {
                    mediaResource.getChildren(image.id)
                        .then(function(data) {
                            $scope.images = data;
                        });
                } else if (image.contentTypeAlias.toLowerCase() == 'image') {
                    $scope.select(image);
                }
            };

        });