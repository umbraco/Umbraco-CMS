
/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.background", function ($scope, $modal) {

    if (!$scope.item.values) {
        $scope.item.values = {
            imageorpattern: ''
        };
    }

    // Open image picker modal
    $scope.open = function (field) {

        $scope.data = {
            newFolder: "",
            modalField: field
        };

        var modalInstance = $modal.open({
            scope: $scope,
            templateUrl: 'myModalContent.html',
            controller: 'tuning.mediapickercontroller',
            resolve: {
                items: function () {
                    return field.imageorpattern;
                }
            }
        });
        modalInstance.result.then(function (selectedItem) {
            field.imageorpattern = selectedItem;
        });
    };

})

.controller('tuning.mediapickercontroller', function ($scope, $modalInstance, items, $http, mediaResource, umbRequestHelper, entityResource, mediaHelper) {

    if (mediaHelper && mediaHelper.registerFileResolver) {
        mediaHelper.registerFileResolver("Umbraco.UploadField", function (property, entity, thumbnail) {
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
        });
    }

    var modalFieldvalue = $scope.data.modalField.imageorpattern;

    $scope.currentFolder = {};
    $scope.currentFolder.children = [];
    $scope.currentPath = [];
    $scope.startNodeId = -1;

    $scope.options = {
        url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
        formData: {
            currentFolder: $scope.startNodeId
        }
    };

    //preload selected item
    $scope.selectedMedia = undefined;

    $scope.submitFolder = function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            $scope.$parent.data.showFolderInput = false;

            if ($scope.$parent.data.newFolder && $scope.$parent.data.newFolder != "") {
                mediaResource
                    .addFolder($scope.$parent.data.newFolder, $scope.currentFolder.id)
                    .then(function (data) {
                        $scope.$parent.data.newFolder = undefined;
                        $scope.gotoFolder(data);
                    });
            }
        }
    };

    $scope.gotoFolder = function (folder) {

        if (!folder) {
            folder = { id: $scope.startNodeId, name: "Media", icon: "icon-folder" };
        }

        if (folder.id > 0) {
            var matches = _.filter($scope.currentPath, function (value, index) {
                if (value.id == folder.id) {
                    value.indexInPath = index;
                    return value;
                }
            });

            if (matches && matches.length > 0) {
                $scope.currentPath = $scope.currentPath.slice(0, matches[0].indexInPath + 1);
            }
            else {
                $scope.currentPath.push(folder);
            }
        }
        else {
            $scope.currentPath = [];
        }

        //mediaResource.rootMedia()
        mediaResource.getChildren(folder.id)
            .then(function (data) {
                folder.children = data.items ? data.items : [];

                angular.forEach(folder.children, function (child) {
                    child.isFolder = child.contentTypeAlias == "Folder" ? true : false;

                    if (!child.isFolder) {
                        child.thumbnail = mediaHelper.resolveFile(child, true);
                        child.image = mediaHelper.resolveFile(child, false);
                    }
                });

                $scope.options.formData.currentFolder = folder.id;
                $scope.currentFolder = folder;
            });
    };

    $scope.iconFolder = "glyphicons-icon folder-open"

    $scope.selectMedia = function (media) {

        if (!media.isFolder) {
            //we have 3 options add to collection (if multi) show details, or submit it right back to the callback
            $scope.selectedMedia = media;
            $scope.data.modalField.imageorpattern = "url(" + $scope.selectedMedia.image + ")";
        }
        else {
            $scope.gotoFolder(media);
        }
    };

    //default root item
    if (!$scope.selectedMedia) {
        $scope.gotoFolder();
    }

    $scope.ok = function () {
        $modalInstance.close($scope.data.modalField.imageorpattern);
    };

    $scope.cancel = function () {
        $scope.data.modalField.imageorpattern = modalFieldvalue;
        $modalInstance.dismiss('cancel');
    };

})