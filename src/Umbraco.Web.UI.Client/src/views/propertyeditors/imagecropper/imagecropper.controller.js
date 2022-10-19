angular.module('umbraco')
    .controller("Umbraco.PropertyEditors.ImageCropperController",
        function ($scope, fileManager, $timeout, mediaHelper) {

            var config = Utilities.copy($scope.model.config);

            $scope.filesSelected = onFileSelected;
            $scope.filesChanged = onFilesChanged;
            $scope.fileUploaderInit = onFileUploaderInit;
            $scope.imageLoaded = imageLoaded;
            $scope.crop = crop;
            $scope.done = done;
            $scope.clear = clear;
            $scope.reset = reset;
            $scope.close = close;
            $scope.isCustomCrop = isCustomCrop;
            $scope.focalPointChanged = focalPointChanged;
            //declare a special method which will be called whenever the value has changed from the server
            $scope.model.onValueChanged = onValueChanged;

            var umbracoSettings = Umbraco.Sys.ServerVariables.umbracoSettings;
            $scope.acceptFileExt = mediaHelper.formatFileTypes(umbracoSettings.imageFileTypes);
            /**
             * Called when the umgImageGravity component updates the focal point value
             * @param {any} left
             * @param {any} top
             */
            function focalPointChanged(left, top) {
                //update the model focalpoint value
                $scope.model.value.focalPoint = {
                    left: left,
                    top: top
                };

                //set form to dirty to track changes
                setDirty();
            }

            /**
             * Used to assign a new model value
             * @param {any} src
             */
            function setModelValueWithSrc(src) {
                if (!$scope.model.value || !$scope.model.value.src) {
                    //we are copying to not overwrite the original config
                    $scope.model.value = Utilities.extend(Utilities.copy($scope.model.config), { src: src });
                }
            }

            /**
             * called whenever the value has changed from the server
             * @param {any} newVal
             * @param {any} oldVal
             */
            function onValueChanged(newVal, oldVal) {
                //clear current uploaded files
                fileManager.setFiles({
                    propertyAlias: $scope.model.alias,
                    culture: $scope.model.culture,
                    segment: $scope.model.segment,
                    files: []
                });
            }

            /**
             * Called when the a new file is selected
             * @param {any} value
             */
            function onFileSelected(value, files) {
                setModelValueWithSrc(value);
                //set form to dirty to track changes
                setDirty();
            }

            function setDirty() {
                if ($scope.imageCropperForm) {
                    $scope.imageCropperForm.modelValue.$setDirty();
                }                
            }

            function imageLoaded(isCroppable, hasDimensions) {
                $scope.isCroppable = isCroppable;
                $scope.hasDimensions = hasDimensions;
            };

            /**
             * Called when the file collection changes
             * @param {any} value
             * @param {any} files
             */
            function onFilesChanged(files) {
                if (files && files[0]) {
                    $scope.imageSrc = files[0].fileSrc;
                    //set form to dirty to track changes
                    setDirty();
                }
            }

            /**
             * Called when the file uploader initializes
             * @param {any} value
             */
            function onFileUploaderInit(value, files) {
                //move previously saved value to the editor
                if ($scope.model.value) {
                    //backwards compat with the old file upload (incase some-one swaps them..)
                    if (Utilities.isString($scope.model.value)) {
                        setModelValueWithSrc($scope.model.value);
                    }
                    else {
                        //sync any config changes with the editor and drop outdated crops
                        _.each($scope.model.value.crops, function (saved) {
                            var configured = _.find(config.crops, function (item) { return item.alias === saved.alias });

                            if (configured && configured.height === saved.height && configured.width === saved.width) {
                                configured.coordinates = saved.coordinates;
                            }
                        });
                        $scope.model.value.crops = config.crops;

                        //restore focalpoint if missing
                        if (!$scope.model.value.focalPoint) {
                            $scope.model.value.focalPoint = { left: 0.5, top: 0.5 };
                        }
                    }

                    //if there are already files in the client assigned then set the src
                    if (files && files[0]) {
                        $scope.imageSrc = files[0].fileSrc;
                    }
                    else {
                        $scope.imageSrc = $scope.model.value.src;
                    }

                }
            }

            /**
             * crop a specific crop
             * @param {any} targetCrop
             */
            function crop(targetCrop) {
                if ($scope.readonly) return;

                if (!$scope.currentCrop) {
                    // clone the crop so we can discard the changes
                    $scope.currentCrop = Utilities.copy(targetCrop);
                    $scope.currentPoint = null;

                    //set form to dirty to track changes
                    setDirty();
                }
                else {
                    // we have a crop open already - close the crop (this will discard any changes made)
                    close();

                    // the crop editor needs a digest cycle to close down properly, otherwise its state
                    // is reused for the new crop... and that's really bad
                    $timeout(function () {
                        crop(targetCrop);
                        $scope.pendingCrop = false;
                    });

                    // this is necessary to keep the screen from flickering too badly while we wait for the new crop to open
                    // - check the view for its usage (basically it makes sure we keep the space reserved for the new crop)
                    $scope.pendingCrop = true;
                }
            };

            /** done cropping */
            function done() {
                if (!$scope.currentCrop) {
                    return;
                }
                // find the original crop by crop alias and update its coordinates
                var editedCrop = _.find($scope.model.value.crops, crop => crop.alias === $scope.currentCrop.alias);
                editedCrop.coordinates = $scope.currentCrop.coordinates;
                $scope.close();

                //set form to dirty to track changes
                setDirty();
            };

            function reset() {
                $scope.currentCrop.coordinates = undefined;
                $scope.done();
            }

            function close() {
                $scope.currentCrop = undefined;
                $scope.currentPoint = undefined;
            }

            /**
             * crop a specific crop
             * @param {any} crop
             */
            function clear(crop) {
                //clear current uploaded files
                fileManager.setFiles({
                    propertyAlias: $scope.model.alias,
                    culture: $scope.model.culture,
                    segment: $scope.model.segment,
                    files: []
                });

                //clear the ui
                $scope.imageSrc = null;
                if ($scope.model.value) {
                    $scope.model.value = null;
                }

                //set form to dirty to track changes
                setDirty();
            };

            function isCustomCrop(crop) {
                return !!crop.coordinates;
            }

            var unsubscribe = $scope.$on("formSubmitting", function () {
                $scope.currentCrop = null;
                $scope.currentPoint = null;
            });

            $scope.$on('$destroy', function () {
                unsubscribe();
            });
        })
    .run(function (mediaHelper, umbRequestHelper) {
        if (mediaHelper && mediaHelper.registerFileResolver) {

            //NOTE: The 'entity' can be either a normal media entity or an "entity" returned from the entityResource
            // they contain different data structures so if we need to query against it we need to be aware of this.
            mediaHelper.registerFileResolver("Umbraco.ImageCropper", function (property, entity, thumbnail) {
                if (property.value && property.value.src) {

                    if (thumbnail === true) {
                        return property.value.src + "?width=500";
                    }
                    else {
                        return property.value.src;
                    }

                    //this is a fallback in case the cropper has been asssigned a upload field
                }
                else if (Utilities.isString(property.value)) {
                    if (thumbnail) {

                        if (mediaHelper.detectIfImageByExtension(property.value)) {

                            var thumbnailUrl = umbRequestHelper.getApiUrl(
                                "imagesApiBaseUrl",
                                "GetBigThumbnail",
                                [{ originalImagePath: property.value }]);

                            return thumbnailUrl;
                        }
                        else {
                            return null;
                        }

                    }
                    else {
                        return property.value;
                    }
                }

                return null;
            });
        }
    });
