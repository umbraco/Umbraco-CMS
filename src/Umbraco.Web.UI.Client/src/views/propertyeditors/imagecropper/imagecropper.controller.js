//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')
    .controller("Umbraco.PropertyEditors.ImageCropperController",
    function ($rootScope, $routeParams, $scope, $log, mediaHelper, cropperHelper, $timeout, editorState, umbRequestHelper, fileManager, angularHelper) {

        var config = angular.copy($scope.model.config);
        $scope.imageIsLoaded = false;

        //move previously saved value to the editor
        if ($scope.model.value) {
            //backwards compat with the old file upload (incase some-one swaps them..)
            if (angular.isString($scope.model.value)) {
                config.src = $scope.model.value;
                $scope.model.value = config;
            } else if ($scope.model.value.crops) {
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

            $scope.imageSrc = $scope.model.value.src;
        }


        //crop a specific crop
        $scope.crop = function (crop) {
            // clone the crop so we can discard the changes
            $scope.currentCrop = angular.copy(crop);
            $scope.currentPoint = undefined;
        };

        //done cropping
        $scope.done = function () {
            if (!$scope.currentCrop) {
                return;
            }
            // find the original crop by crop alias and update its coordinates
            var editedCrop = _.find($scope.model.value.crops, function(crop) {
                return crop.alias === $scope.currentCrop.alias;
            });
            editedCrop.coordinates = $scope.currentCrop.coordinates;
            $scope.close();
            angularHelper.getCurrentForm($scope).$setDirty();
        };

        //reset the current crop
        $scope.reset = function() {
            $scope.currentCrop.coordinates = undefined;
            $scope.done();
        }

        //close crop overlay
        $scope.close = function (crop) {
            $scope.currentCrop = undefined;
            $scope.currentPoint = undefined;
        };

        //crop a specific crop
        $scope.clear = function (crop) {
            //clear current uploaded files
            fileManager.setFiles($scope.model.alias, []);

            //clear the ui
            $scope.imageSrc = undefined;
            if ($scope.model.value) {
                delete $scope.model.value;
            }

            // set form to dirty to tricker discard changes dialog
            var currForm = angularHelper.getCurrentForm($scope);
            currForm.$setDirty();
        };

        //show previews
        $scope.togglePreviews = function () {
            if ($scope.showPreviews) {
                $scope.showPreviews = false;
                $scope.tempShowPreviews = false;
            } else {
                $scope.showPreviews = true;
            }
        };

        $scope.imageLoaded = function (isCroppable, hasDimensions) {
            $scope.imageIsLoaded = true;
            $scope.isCroppable = isCroppable;
            $scope.hasDimensions = hasDimensions;
        };

        $scope.focalPointChanged = function () {
            angularHelper.getCurrentForm($scope).$setDirty();
        }

        //on image selected, update the cropper
        $scope.$on("filesSelected", function (ev, args) {
            $scope.model.value = config;

            if (args.files && args.files[0]) {

                fileManager.setFiles($scope.model.alias, args.files);

                var reader = new FileReader();
                reader.onload = function (e) {

                    $scope.$apply(function () {
                        $scope.imageSrc = e.target.result;
                    });

                };

                reader.readAsDataURL(args.files[0]);
            }
        });


        //here we declare a special method which will be called whenever the value has changed from the server
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //clear current uploaded files
            fileManager.setFiles($scope.model.alias, []);
        };

        var unsubscribe = $scope.$on("formSubmitting", function () {
            $scope.done();
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
                        return property.value.src + "?width=500&mode=max&animationprocessmode=first";
                    }
                    else {
                        return property.value.src;
                    }

                    //this is a fallback in case the cropper has been asssigned a upload field
                }
                else if (angular.isString(property.value)) {
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
