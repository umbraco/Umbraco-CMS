angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, userService, editorService, localizationService) {

        $scope.control.icon = $scope.control.icon || 'icon-picture';

        $scope.thumbnailUrl = getThumbnailUrl();

        if (!$scope.model.config.startNodeId) {
            if ($scope.model.config.ignoreUserStartNodes === true) {
                $scope.model.config.startNodeId = -1;
                $scope.model.config.startNodeIsVirtual = true;

            } else {
                userService.getCurrentUser().then(userData => {
                    $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                    $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
                });
            }
        }

        $scope.setImage = function() {
            var startNodeId = $scope.model.config && $scope.model.config.startNodeId ? $scope.model.config.startNodeId : null;

            var mediaPicker = {
                startNodeId: startNodeId,
                startNodeIsVirtual: startNodeId ? $scope.model.config.startNodeIsVirtual : null,
                cropSize: $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : null,
                showDetails: true,
                disableFolderSelect: true,
                onlyImages: true,
                dataTypeKey: $scope.model.dataTypeKey,
                submit: model => {
                    updateControlValue(model.selection[0]);
                    editorService.close();
                },
                close: () => editorService.close()
            };

            editorService.mediaPicker(mediaPicker);
        };

        $scope.editImage = function() {

            const mediaCropDetailsConfig = {
                size: 'small',
                target: $scope.control.value,
                submit: model => {
                    updateControlValue(model.target);
                    editorService.close();
                },
                close: () => editorService.close()
            };

            localizationService.localize('defaultdialogs_editSelectedMedia').then(value => {
                mediaCropDetailsConfig.title = value;
                editorService.mediaCropDetails(mediaCropDetailsConfig);
            });
        }

        /**
         *
         */
        function getThumbnailUrl() {

            if ($scope.control.value && $scope.control.value.image) {
                var url = $scope.control.value.image;

                if ($scope.control.editor.config && $scope.control.editor.config.size){
                    if ($scope.control.value.coordinates) {
                        // New way, crop by percent must come before width/height.
                        var coords = $scope.control.value.coordinates;
                        url += `?cc=${coords.x1},${coords.y1},${coords.x2},${coords.y2}`;
                    } else {
                        // Here in order not to break existing content where focalPoint were used.
                        if ($scope.control.value.focalPoint) {
                            url += `?rxy=${$scope.control.value.focalPoint.left},${$scope.control.value.focalPoint.top}`;
                        } else {
                            // Prevent black padding and no crop when focal point not set / changed from default
                            url += '?rxy=0.5,0.5';
                        }
                    }

                    url += '&width=' + $scope.control.editor.config.size.width;
                    url += '&height=' + $scope.control.editor.config.size.height;
                }

                // set default size if no crop present (moved from the view)
                if (url.includes('?') === false)
                {
                    url += '?width=800'
                }

                return url;
            }

            return null;
        }

        /**
         *
         * @param {object} selectedImage
         */
        function updateControlValue(selectedImage) {
            // we could apply selectedImage directly to $scope.control.value,
            // but this allows excluding fields in future if needed
            $scope.control.value = {
                focalPoint: selectedImage.focalPoint,
                coordinates: selectedImage.coordinates,
                id: selectedImage.id,
                udi: selectedImage.udi,
                image: selectedImage.image,
                caption: selectedImage.caption,
                altText: selectedImage.altText
            };
            $scope.thumbnailUrl = getThumbnailUrl();
        }
    });
