//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.MediaCropperController",
    function ($rootScope, $scope, dialogService, entityResource, mediaResource, mediaHelper, $timeout, userService, $location, localizationService) {

        var config = angular.copy($scope.model.config);

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;
        var disableFolderSelect = $scope.model.config.disableFolderSelect && $scope.model.config.disableFolderSelect !== '0' ? true : false;

        if (!$scope.model.config.startNodeId) {
            userService.getCurrentUser().then(function(userData) {
                $scope.model.config.startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                $scope.model.config.startNodeIsVirtual = userData.startMediaIds.length !== 1;
            });
        }

        function setupViewModel() {
            $scope.images = [];
            $scope.ids = [];

            $scope.isMultiPicker = multiPicker;

            if ($scope.model.value) {
                var ids = $scope.model.value.map(function (data) { return data.udi; });

                //NOTE: We need to use the entityResource NOT the mediaResource here because
                // the mediaResource has server side auth configured for which the user must have
                // access to the media section, if they don't they'll get auth errors. The entityResource
                // acts differently in that it allows access if the user has access to any of the apps that
                // might require it's use. Therefore we need to use the metaData property to get at the thumbnail
                // value.

                entityResource.getByIds(ids, "Media").then(function (medias) {

                    // The service only returns item results for ids that exist (deleted items are silently ignored).
                    // This results in the picked items value to be set to contain only ids of picked items that could actually be found.
                    // Since a referenced item could potentially be restored later on, instead of changing the selected values here based
                    // on whether the items exist during a save event - we should keep "placeholder" items for picked items that currently
                    // could not be fetched. This will preserve references and ensure that the state of an item does not differ depending
                    // on whether it is simply resaved or not.
                    // This is done by remapping the int/guid ids into a new array of items, where we create "Deleted item" placeholders
                    // when there is no match for a selected id. This will ensure that the values being set on save, are the same as before.

                    medias = _.map(ids,
                        function (id) {
                            var found = _.find(medias,
                                function (m) {
                                    // We could use coercion (two ='s) here .. but not sure if this works equally well in all browsers and
                                    // it's prone to someone "fixing" it at some point without knowing the effects. Rather use toString()
                                    // compares and be completely sure it works.
                                    return m.udi.toString() === id.toString() || m.id.toString() === id.toString();
                                });
                            if (found) {
                                return found;
                            } else {
                                return {
                                    name: localizationService.dictionary.mediaPicker_deletedItem,
                                    id: $scope.model.config.idType !== "udi" ? id : null,
                                    udi: $scope.model.config.idType === "udi" ? id : null,
                                    icon: "icon-picture",
                                    thumbnail: null,
                                    trashed: true
                                };
                            }
                        });

                    _.each(medias,
                        function (media, i) {
                            // if there is no thumbnail, try getting one if the media is not a placeholder item
                            if (!media.thumbnail && media.id && media.metaData) {
                                media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                                media.file = mediaHelper.resolveFileFromEntity(media, false);
                            }

                            $scope.images.push(media);
                            $scope.ids.push(media.udi);
                        });

                    $scope.sync();

                    angular.forEach($scope.model.value, function (cropDataSet, i) {
                        $scope.resetCropFromUdi(cropDataSet.udi, cropDataSet);
                    });
                });
            }
            else {
                $scope.model.value = [];
            }
        }

        setupViewModel();

        $scope.remove = function(index) {
            $scope.images.splice(index, 1);
            $scope.ids.splice(index, 1);
            $scope.sync();
        };

        $scope.goToItem = function(item) {
            $location.path('media/media/edit/' + item.id);
        };

       $scope.add = function() {

           $scope.mediaPickerOverlay = {
               view: "mediapicker",
               title: "Select media",
               startNodeId: $scope.model.config.startNodeId,
               startNodeIsVirtual: $scope.model.config.startNodeIsVirtual,
               multiPicker: multiPicker,
               onlyImages: true,
               disableFolderSelect: true,
               show: true,
               submit: function(model) {

                   _.each(model.selectedImages, function(media, i) {
                       // if there is no thumbnail, try getting one if the media is not a placeholder item
                       if (!media.thumbnail && media.id && media.metaData) {
                           media.thumbnail = mediaHelper.resolveFileFromEntity(media, true);
                       }

                       $scope.images.push(media);

                       $scope.ids.push(media.udi);
                   });

                   $scope.sync();

                   _.each(model.selectedImages, function (media, i) {
                       $scope.resetCropFromMedia(media);
                   });

                   $scope.mediaPickerOverlay.show = false;
                   $scope.mediaPickerOverlay = null;
               }
           };
        };

        $scope.cropItem = function (item) {

            var cropDataSet = $scope.model.value.filter(function (data) { return data.udi == item.udi });
            if (angular.isString(item.file)) {
                cropDataSet[0].src = item.file;
            }
            else {
                cropDataSet[0].src = item.file.src;
            }

            $scope.mediaPickerOverlay = {
                view: "views/propertyeditors/imagecropper/imagecropper.html",
                title: "Crop settings",
                config: config,
                value: cropDataSet[0],
                show: true,
                submit: function (model) {

                    _.each($scope.model.value, function (crop, i) {
                        if (crop.udi == item.udi) {
                            $scope.model.value[i] = crop;
                        }
                    });

                    $scope.sync();

                    $scope.mediaPickerOverlay.show = false;
                    $scope.mediaPickerOverlay = null;
                }
            };
        }

       $scope.sortableOptions = {
           disabled: !$scope.isMultiPicker,
           items: "li:not(.add-wrapper)",
           cancel: ".unsortable",
           update: function(e, ui) {
               var r = [];
               // TODO: Instead of doing this with a half second delay would be better to use a watch like we do in the
               // content picker. Then we don't have to worry about setting ids, render models, models, we just set one and let the
               // watch do all the rest.
                $timeout(function(){
                    angular.forEach($scope.images, function(value, key) {
                        r.push(value.udi);
                    });
                    $scope.ids = r;
                    $scope.sync();
                }, 500, false);
            }
        };

        $scope.resetCropFromUdi = function (udi, cropDataSet) {
            entityResource.getById(udi, "Media").then(function (media) {
                $scope.resetCropFromMedia(media, cropDataSet);
            });
        };

        $scope.resetCropFromMedia = function (media, cropDataSet) {
            if (cropDataSet === undefined) {
                if (media.file === undefined) {
                    var newCropDataSet = mediaHelper.resolveFileFromEntity(media, false);
                    if (!angular.isString(newCropDataSet)) {
                        cropDataSet = newCropDataSet;
                    }
                }
                else if (!angular.isString(media.file)) {
                    cropDataSet = media.file;
                }
            }

            if (cropDataSet !== undefined) {


                var savedUdis = $scope.model.value.map(function (value) { return value.udi; });
                var cropDataSetIndex = savedUdis.indexOf(media.udi);

                if (cropDataSetIndex > -1) {

                    //sync any config changes with the editor and drop outdated crops
                    _.each(cropDataSet.crops, function (saved) {
                        var configured = _.find(config.crops, function (item) { return item.alias === saved.alias });

                        if (configured && configured.height === saved.height && configured.width === saved.width) {
                            configured.coordinates = saved.coordinates;
                        }
                    });
                    cropDataSet.crops = config.crops;

                    //restore focalpoint if missing
                    if (!cropDataSet.focalPoint) {
                        cropDataSet.focalPoint = config.focalPoint;
                    }

                    $scope.model.value[cropDataSetIndex] = cropDataSet;
                    $scope.model.value[cropDataSetIndex].udi = media.udi;
                }

            }
        }

        $scope.sync = function () {
            var newValue = [];
            angular.forEach($scope.ids, function (id, i) {


                foundCropDataSet = $scope.model.value.filter(function (value) { return value.udi == id; });
                if (foundCropDataSet.length > 0) {
                    newValue.push(foundCropDataSet[0]);
                }
                else {
                    var cropDataSet = {
                        udi: id,
                        crops: config.crops,
                        focalPoint: config.focalPoint
                    }
                    newValue.push(cropDataSet);
                }

            });
            $scope.model.value = newValue;
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
