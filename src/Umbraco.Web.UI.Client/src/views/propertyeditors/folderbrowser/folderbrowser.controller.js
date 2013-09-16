angular.module("umbraco")
.controller("Umbraco.Editors.FolderBrowserController",
    function ($rootScope, $scope, $routeParams, umbRequestHelper, mediaResource, imageHelper) {
        var dialogOptions = $scope.$parent.dialogOptions;
        $scope.options = {
            url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
            autoUpload: true,
            formData:{
                currentFolder: $routeParams.id
            }
        };

        $scope.loadChildren = function(id){
            mediaResource.getChildren(id)
                .then(function(data) {
                    $scope.images = data;
                    //update the thumbnail property
                    _.each($scope.images, function(img) {
                        img.thumbnail = imageHelper.getThumbnail({ imageModel: img, scope: $scope });
                    });
                });    
        };

        $scope.$on('fileuploadstop', function(event, files){
            $scope.loadChildren($scope.options.formData.currentFolder);
        });
        
        //init load
        $scope.loadChildren($routeParams.id);
    }
);