(function () {
    'use strict';

    function MediaBlockEditor($scope, mediaResource, mediaHelper) {

        console.log("MediaBlockEditor", $scope.block.data)

        const bc = this;
        
        $scope.$watch("block.data.media", function(newValue, oldValue) {

            if (newValue !== oldValue) {
                bc.retrieveMedia();
            }
           
        });

        bc.retrieveMedia = function() {
            console.log("retrieveMedia", $scope.block.data.image[0]);

            if($scope.block.data.image.length > 0) {
                mediaResource.getById($scope.block.data.image[0].mediaKey).then(function (mediaEntity) {
                    

                    var mediaPath = mediaEntity.mediaLink;

                    //set a property on the 'scope' for the returned media object
                    bc.icon = "picture";
                    bc.mediaName = "Name NAME TODO"
                    bc.fileExtension = mediaHelper.getFileExtension(mediaPath);
                    bc.isImage = mediaHelper.detectIfImageByExtension(mediaPath);
                    bc.imageSource = mediaHelper.getThumbnailFromPath(mediaPath);
                    console.log("Got Media");
                    console.log(mediaEntity);
                    console.log(bc);
                });
            }
        }

        bc.retrieveMedia();

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.MediaBlockEditor", MediaBlockEditor);

})();
