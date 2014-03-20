//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco')   
    .controller("Umbraco.PropertyEditors.ImageCropperController",
    function ($rootScope, $routeParams, $scope, $log, mediaHelper, cropperHelper, $timeout, editorState, umbRequestHelper, fileManager) {

        var config =  angular.copy($scope.model.config);
        
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
            $scope.currentCrop = crop;
            $scope.currentPoint = undefined;
        };

        //done cropping
        $scope.done = function () {
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
    })
    .run(function(mediaHelper, umbRequestHelper){
        if(mediaHelper && mediaHelper.registerFileResolver){
            mediaHelper.registerFileResolver("Umbraco.ImageCropper", function (property, entity, thumbnail) {
                    if (property.value.src) {
                        
                        if(thumbnail === true){
                            return property.value.src + "?width=600&mode=max";
                        }else{
                            return property.value.src;
                        }
                        
                    //this is a fallback in case the cropper has been asssigned a upload field
                    } else if (angular.isString(property.value)) {
                        if(thumbnail){
                            var thumbnailUrl = umbRequestHelper.getApiUrl(
                                "imagesApiBaseUrl",
                                "GetBigThumbnail",
                                [{ mediaId: entity.id }]);

                            return thumbnailUrl;
                        }else{
                            return property.value;
                        }
                    }
                });
        }
    });