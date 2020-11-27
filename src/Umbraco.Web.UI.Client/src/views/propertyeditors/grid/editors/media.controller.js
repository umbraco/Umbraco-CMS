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
            var value = $scope.control.value;
            var target = value
                ? {
                    udi: value.udi,
                    url: value.image,
                    image: value.image,
                    focalPoint: value.focalPoint,
                    coordinates: value.coordinates
                }
                : null;
            var mediaPicker = {
                startNodeId: startNodeId,
                startNodeIsVirtual: startNodeIsVirtual,
                cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined,
                showDetails: true,
                disableFolderSelect: true,
                onlyImages: true,
                dataTypeKey: $scope.model.dataTypeKey,
                currentTarget: target,
                submit: function(model) {
                    var selectedImage = model.selection[0];
                   
                    $scope.control.value = {
                        focalPoint: selectedImage.focalPoint,
                        coordinates: selectedImage.coordinates,
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
                    if ($scope.control.value.coordinates) {
                        // New way, crop by percent must come before width/height.
                        var coords = $scope.control.value.coordinates;
                        url += "?crop=" + coords.x1 + "," + coords.y1 + "," + coords.x2 + "," + coords.y2 + "&cropmode=percentage";
                    } else {
                        // Here in order not to break existing content where focalPoint were used.
                        // For some reason width/height have to come first when mode=crop.
                        if ($scope.control.value.focalPoint) {
                            url += "?center=" + $scope.control.value.focalPoint.top + "," + $scope.control.value.focalPoint.left;
                            url += "&mode=crop";
                        } else {
                            // Prevent black padding and no crop when focal point not set / changed from default
                            url += "?center=0.5,0.5&mode=crop";
                        }
                    }
                    url += "&width=" + $scope.control.editor.config.size.width;
                    url += "&height=" + $scope.control.editor.config.size.height;
                    url += "&animationprocessmode=first";

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
