//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaPickerController",
    function ($rootScope, $scope, dialogService, entityResource, mediaResource, mediaHelper, $timeout, userService, angularHelper) {

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;

        if (!$scope.model.config.startNodeId) {
            userService.getCurrentUser().then(function (userData) {
                $scope.model.config.startNodeId = userData.startMediaId;
            });
        }
            

         
        function setupViewModel() {
            $scope.images = [];
            $scope.ids = []; 

            if ($scope.model.value) {
                var ids = $scope.model.value.split(',');

                //NOTE: We need to use the entityResource NOT the mediaResource here because
                // the mediaResource has server side auth configured for which the user must have
                // access to the media section, if they don't they'll get auth errors. The entityResource
                // acts differently in that it allows access if the user has access to any of the apps that
                // might require it's use. Therefore we need to use the metatData property to get at the thumbnail
                // value.

                entityResource.getByIds(ids, "Media").then(function (medias) {

                    _.each(medias, function (media, i) {
                        
                        //only show non-trashed items
                        if (media.parentId >= -1) {

                            if (!media.thumbnail) { 
                                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                            }

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
            angularHelper.getCurrentForm($scope).$setDirty();
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

                        if (!media.thumbnail) {
                            media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                        }

                        $scope.images.push(media);
                        $scope.ids.push(media.id);
                        angularHelper.getCurrentForm($scope).$setDirty();
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
