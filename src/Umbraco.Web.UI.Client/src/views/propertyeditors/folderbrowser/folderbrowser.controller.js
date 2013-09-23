angular.module("umbraco")
.controller("Umbraco.Editors.FolderBrowserController",
    function ($rootScope, $scope, assetsService, $routeParams, $timeout, umbRequestHelper, mediaResource, imageHelper) {
        var dialogOptions = $scope.$parent.dialogOptions;

        $scope.filesUploading = [];

        $scope.options = {
            url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
            autoUpload: true,
            disableImageResize: /Android(?!.*Chrome)|Opera/
            .test(window.navigator.userAgent),
            previewMaxWidth: 100,
            previewMaxHeight: 100,
            previewCrop: true,
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
            $scope.queue = [];
        });

        $scope.$on('fileuploadprocessalways', function(e,data) {
            var i;
            console.log('processing');

            $scope.$apply(function() {
                $scope.filesUploading.push(data.files[data.index].preview);
            });
        })


        // All these sit-ups are to add dropzone area and make sure it gets removed if dragging is aborted! 
        $scope.$on('fileuploaddragover', function(event, files) {
            if (!$scope.dragClearTimeout) {
                $scope.$apply(function() {
                    $scope.dropping = true;
                });
            } else {
                $timeout.cancel($scope.dragClearTimeout);
            }
            $scope.dragClearTimeout = $timeout(function () {
                $scope.dropping = null;
                $scope.dragClearTimeout = null;
            }, 100);
        })
        
        //init load
        $scope.loadChildren($routeParams.id);
    }
);