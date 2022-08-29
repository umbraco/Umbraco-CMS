(function () {
    'use strict';

    function HeroBlockEditor($scope, mediaResource, mediaHelper) {

        var unsubscribe = [];

        const bc = this;
        
        $scope.$watch("block.data.image", function(newValue, oldValue) {
            if (newValue !== oldValue) {
                bc.retrieveMedia();
            }
        }, true);

        bc.retrieveMedia = function() {

            if($scope.block.data.image && $scope.block.data.image.length > 0) {
                mediaResource.getById($scope.block.data.image[0].mediaKey).then(function (mediaEntity) {
                    
                    var mediaPath = mediaEntity.mediaLink;

                    //set a property on the 'scope' for the returned media object.
                    bc.mediaName = mediaEntity.name;
                    bc.isImage = mediaHelper.detectIfImageByExtension(mediaPath);
                    bc.imageSource = mediaPath;
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

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.HeroBlockEditor", HeroBlockEditor);

})();
