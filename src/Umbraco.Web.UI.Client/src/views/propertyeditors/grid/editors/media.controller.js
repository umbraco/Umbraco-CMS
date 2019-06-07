angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
        function ($scope, $timeout, userService, editorService) {
            var ignoreUserStartNodes = Object.toBoolean($scope.model.config.ignoreUserStartNodes);

            function updateControlValue(selectedImage) {
                $scope.control.value = {
                    focalPoint: selectedImage.focalPoint,
                    id: selectedImage.id,
                    udi: selectedImage.udi,
                    image: selectedImage.image,
                    caption: selectedImage.caption,
                    altText: selectedImage.altText
                };

                setThumbnailUrl();
            }

            function setThumbnailUrl() {

                var url = null;

                if ($scope.control.value && $scope.control.value.image) {
                    url = $scope.control.value.image;

                    if ($scope.control.editor.config && $scope.control.editor.config.size) {
                        url += "?width=" + $scope.control.editor.config.size.width;
                        url += "&height=" + $scope.control.editor.config.size.height;
                        url += "&animationprocessmode=first";

                        if ($scope.control.value.focalPoint) {
                            url += "&center=" + $scope.control.value.focalPoint.top + "," + $scope.control.value.focalPoint.left;
                            url += "&mode=crop";
                        }
                    }

                    // set default size if no crop present (moved from the view)
                    if (url.indexOf('?') == -1) {
                        url += "?width=800&upscale=false&animationprocessmode=false"
                    }
                }

                $scope.thumbnailUrl = url
            };
    
            setThumbnailUrl();

            if (!$scope.model.config.startNodeId) {
                if (ignoreUserStartNodes === true) {
                    $scope.model.config.startNodeId = -1;
                    $scope.model.config.startNodeIsVirtual = true;

                } else {
                    userService.getCurrentUser().then(function (userData) {
                        $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                        $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                    });
                }
            }

            $scope.editImage = function () {
                var mediaPickerDetails = {
                    itemDetails: $scope.control.value,
                    imageUrl: $scope.control.value.image,
                    cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined,
                    submit: function (model) {
                        updateControlValue(model.itemDetails);
                        editorService.close();
                    },
                    close: function (model) {
                        updateControlValue(model.itemDetails);
                        editorService.close();
                    }
                };
                
                editorService.mediaPickerDetails(mediaPickerDetails);
            };

            $scope.setImage = function () {
                var startNodeId = $scope.model.config && $scope.model.config.startNodeId ? $scope.model.config.startNodeId : undefined;
                var startNodeIsVirtual = startNodeId ? $scope.model.config.startNodeIsVirtual : undefined;
                var mediaPicker = {
                    startNodeId: startNodeId,
                    startNodeIsVirtual: startNodeIsVirtual,
                    ignoreUserStartNodes: ignoreUserStartNodes,
                    cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined,
                    showDetails: true,
                    disableFolderSelect: true,
                    onlyImages: true,
                    submit: function (model) {
                        updateControlValue(model.selection[0]);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                }

                editorService.mediaPicker(mediaPicker);
            };
        });
