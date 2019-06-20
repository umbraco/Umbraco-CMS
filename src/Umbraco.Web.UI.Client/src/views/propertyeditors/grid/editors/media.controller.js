angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, $rootScope, $timeout, userService) {

        if (!$scope.model.config.startNodeId) {
            if ($scope.model.config.ignoreUserStartNodes === "1" ) {
                $scope.model.config.startNodeId = -1;
                $scope.model.config.startNodeIsVirtual = true;

            } else {
                userService.getCurrentUser().then(function (userData) {
                    $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                    $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                });
            }
        }

        $scope.setImage = function(){
            $scope.mediaPickerOverlay = {};
            $scope.mediaPickerOverlay.view = "mediapicker";
            $scope.mediaPickerOverlay.startNodeId = $scope.model.config && $scope.model.config.startNodeId ? $scope.model.config.startNodeId : undefined;
            $scope.mediaPickerOverlay.startNodeIsVirtual = $scope.mediaPickerOverlay.startNodeId ? $scope.model.config.startNodeIsVirtual : undefined;
            $scope.mediaPickerOverlay.dataTypeId = $scope.model.dataTypeId;
            $scope.mediaPickerOverlay.cropSize = $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined;
            $scope.mediaPickerOverlay.showDetails = true;
            $scope.mediaPickerOverlay.disableFolderSelect = true;
            $scope.mediaPickerOverlay.onlyImages = true;
            $scope.mediaPickerOverlay.show = true;

            $scope.mediaPickerOverlay.submit = function(model) {
                var selectedImage = model.selectedImages[0];

                $scope.control.value = {
                    focalPoint: selectedImage.focalPoint,
                    id: selectedImage.id,
                    udi: selectedImage.udi,
                    image: selectedImage.image,
                    altText: selectedImage.altText
                };

                $scope.setUrl();

                $scope.mediaPickerOverlay.show = false;
                $scope.mediaPickerOverlay = null;
            };

            $scope.mediaPickerOverlay.close = function(oldModel) {
                $scope.mediaPickerOverlay.show = false;
                $scope.mediaPickerOverlay = null;
            };
        };

        $scope.setUrl = function(){

            if($scope.control.value.image){
                var url = $scope.control.value.image;

                if($scope.control.editor.config && $scope.control.editor.config.size){
                    url += "?width=" + $scope.control.editor.config.size.width;
                    url += "&height=" + $scope.control.editor.config.size.height;
                    url += "&animationprocessmode=first";

                    if($scope.control.value.focalPoint){
                        url += "&center=" + $scope.control.value.focalPoint.top +"," + $scope.control.value.focalPoint.left;
                        url += "&mode=crop";
                    }
                }

                // set default size if no crop present (moved from the view)
                if (url.indexOf('?') == -1)
                {
                    url += "?width=800&upscale=false&animationprocessmode=false"
                }
                $scope.url = url;
            }
        };

        $timeout(function(){
            if($scope.control.$initializing){
                $scope.setImage();
            }else if($scope.control.value){
                $scope.setUrl();
            }
        }, 200);
});
