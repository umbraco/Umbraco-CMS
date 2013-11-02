//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function($rootScope, $scope, dialogService, mediaResource, imageHelper, $log) {


        function setupViewModel() {
            $scope.images = [];
            $scope.ids = [];

            if ($scope.model.value) {
                $scope.ids = $scope.model.value.split(',');

                mediaResource.getByIds($scope.ids).then(function (medias) {
                    //img.media = media;
                    _.each(medias, function (media, i) {
                        var img = {};
                        img.src = imageHelper.getImagePropertyValue({ imageModel: media });
                        img.thumbnail = imageHelper.getThumbnailFromPath(img.src);
                        $scope.images.push(img);
                    });
                });
            }
        }

        setupViewModel();

        $scope.remove = function(index) {
            $scope.images.splice(index, 1);
            $scope.ids.splice(index, 1);
            $scope.sync();
        };

        $scope.add = function() {
            dialogService.mediaPicker({
                multiPicker: true,
                callback: function(data) {
                    
                    _.each(data, function(media, i) {
                        var img = {};
                        img.id = media.id;
                        img.src = imageHelper.getImagePropertyValue({ imageModel: media });
                        img.thumbnail = imageHelper.getThumbnailFromPath(img.src);
                        
                        $scope.images.push(img);
                        $scope.ids.push(img.id);
                    });

                    $scope.sync();
                }
            });
        };

       $scope.sortableOptions = {
            update: function(e, ui) {
                var r = [];
                angular.forEach($scope.renderModel, function(value, key){
                    r.push(value.id);
                });

                $scope.ids = r;
                $scope.sync();
            }
        };

        $scope.sync = function() {
            $scope.model.value = $scope.ids.join();
        };

        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            setupViewModel();
        };

    });