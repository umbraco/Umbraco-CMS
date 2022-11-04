(function () {
    'use strict';

    function MediaBlockEditor($scope, mediaResource, mediaHelper) {

        var unsubscribe = [];

        const bc = this;

        bc.hasImage = false;
        
        $scope.$watch("block.data.image", function(newValue, oldValue) {
            if (newValue !== oldValue) {
                bc.retrieveMedia();
            }
        }, true);

        bc.retrieveMedia = function() {

            bc.hasImage = false;

            if($scope.block.data.image && $scope.block.data.image.length > 0) {
                mediaResource.getById($scope.block.data.image[0].mediaKey).then(function (mediaEntity) {
                    
                    bc.hasImage = true;
                    var mediaPath = mediaEntity.mediaLink;

                    //set a property on the 'scope' for the returned media object
                    bc.icon = mediaEntity.contentType.icon;
                    bc.mediaName = mediaEntity.name;
                    bc.fileExtension = mediaHelper.getFileExtension(mediaPath);
                    bc.isImage = mediaHelper.detectIfImageByExtension(mediaPath);
                    bc.imageSource = mediaHelper.getThumbnailFromPath(mediaPath);
                });
            }
        }

        bc.retrieveMedia();



        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.MediaBlockEditor", MediaBlockEditor);

})();
