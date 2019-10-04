angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, $timeout, userService, editorService) {
        
        
        $scope.thumbnailUrl = getThumbnailUrl();
        
        
        if (!$scope.model.config.startNodeId) {
            if ($scope.model.config.ignoreUserStartNodes === true) {
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
            var startNodeId = $scope.model.config && $scope.model.config.startNodeId ? $scope.model.config.startNodeId : undefined;
            var startNodeIsVirtual = startNodeId ? $scope.model.config.startNodeIsVirtual : undefined;

            var mediaPicker = {
                startNodeId: startNodeId,
                startNodeIsVirtual: startNodeIsVirtual,
                cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined,
                showDetails: true,
                disableFolderSelect: true,
                onlyImages: true,
                dataTypeKey: $scope.model.dataTypeKey,
                submit: function(model) {
                    var selectedImage = model.selection[0];
                   
                    $scope.control.value = {
                        focalPoint: selectedImage.focalPoint,
                        id: selectedImage.id,
                        udi: selectedImage.udi,
                        image: selectedImage.image,
                        caption: selectedImage.altText
                    };
                    
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            }
            
            editorService.mediaPicker(mediaPicker);
        };
        
        $scope.$watch('control.value', function(newValue, oldValue) {
            if(angular.equals(newValue, oldValue)){
                return; // simply skip that
            }
            
            $scope.thumbnailUrl = getThumbnailUrl();
        }, true);
        
        function getThumbnailUrl() {

            if($scope.control.value && $scope.control.value.image) {
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
                return url;
            }
            
            return null;
        };

});
