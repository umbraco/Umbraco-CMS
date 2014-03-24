//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function($rootScope, $scope, dialogService, mediaResource, mediaHelper, $timeout) {

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker !== '0' ? true : false;

        if (!$scope.model.config.startNodeId)
             $scope.model.config.startNodeId = -1;

         
        function setupViewModel() {
            $scope.images = [];
            $scope.ids = []; 

            if ($scope.model.value) {
                var ids = $scope.model.value.split(',');

                mediaResource.getByIds(ids).then(function (medias) {
                    //img.media = media;
                    _.each(medias, function (media, i) {
                        
                        //only show non-trashed items
                        if(media.parentId >= -1){
                            if(!media.thumbnail){
                                media.thumbnail = mediaHelper.resolveFile(media, true);
                            }

                            //media.src = mediaHelper.getImagePropertyValue({ imageModel: media });
                            //media.thumbnail = mediaHelper.getThumbnailFromPath(media.src);
                            $scope.images.push(media);
                            $scope.ids.push(media.id);   
                        }
                    });

                    $scope.sync();
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
                startNodeId: $scope.model.config.startNodeId,
                multiPicker: multiPicker,
                callback: function(data) {
                    
                    //it's only a single selector, so make it into an array
                    if (!multiPicker) {
                        data = [data];
                    }
                    
                    _.each(data, function(media, i) {

                        if(!media.thumbnail){
                            media.thumbnail = mediaHelper.resolveFile(media, true);
                        }
                        
                        $scope.images.push(media);
                        $scope.ids.push(media.id);
                    });

                    $scope.sync();
                }
            });
        };

       $scope.sortableOptions = {
           update: function(e, ui) {
               var r = [];
               //TODO: Instead of doing this with a half second delay would be better to use a watch like we do in the 
               // content picker. THen we don't have to worry about setting ids, render models, models, we just set one and let the 
               // watch do all the rest.
                $timeout(function(){
                    angular.forEach($scope.images, function(value, key){
                        r.push(value.id);
                    });

                    $scope.ids = r;
                    $scope.sync();
                }, 500, false);
                
            }
        };

        $scope.sync = function() {
            $scope.model.value = $scope.ids.join();
        };

        $scope.showAdd = function () {
            if (!multiPicker) {
                if ($scope.model.value && $scope.model.value !== "") {
                    return false;
                }
            }
            return true;
        };

        //here we declare a special method which will be called whenever the value has changed from the server
        //this is instead of doing a watch on the model.value = faster
        $scope.model.onValueChanged = function (newVal, oldVal) {
            //update the display val again if it has changed from the server
            setupViewModel();
        };

    });