
/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.background", function ($scope, dialogService) {

    if (!$scope.item.values) {
        $scope.item.values = {
            imageorpattern: '',
            color: ''
        };
    }

    $scope.open = function (field) {

        var config = {
            template: "mediaPickerModal.html",
            change: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            callback: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            cancel: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            dialogData: $scope.googleFontFamilies,
            dialogItem: $scope.item.values.imageorpattern
        };

        dialogService.open(config);

    };

})

.controller('canvasdesigner.mediaPickerModal', function ($scope, $http, mediaResource, umbRequestHelper, entityResource, mediaHelper) {

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

    var modalFieldvalue = $scope.dialogItem;

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
                        angular.forEach(child.properties, function (property) {       
                            if (property.alias == "umbracoFile" && property.value)
                            {
                                child.thumbnail = mediaHelper.resolveFile(child, true);
                                child.image = property.value;
                            }
                        })
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
            modalFieldvalue = "url(" + $scope.selectedMedia.image + ")";
            $scope.change(modalFieldvalue);
        }
        else {
            $scope.gotoFolder(media);
        }

    };

    //default root item
    if (!$scope.selectedMedia) {
        $scope.gotoFolder();
    }

    $scope.submitAndClose = function () {
        if (modalFieldvalue != "") {
            $scope.submit(modalFieldvalue);
        } else {
            $scope.cancel();
        }

    };

    $scope.cancelAndClose = function () {
        $scope.cancel();
    }

})
