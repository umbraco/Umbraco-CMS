(function () {
    "use strict";
    function RedirectUrlPickerController($scope, entityResource, editorState) {

        $scope.urlRegex = new RegExp("([\/]{1})[^\/](.*)");
        $scope.contentItem = null;
        $scope.loaded = false;
        $scope.enablePicker = editorState == null || editorState.current == null;

        $scope.removeContentItem = function () {
            if ($scope.enablePicker) {
                $scope.contentItem = null;
            }
        };

        $scope.openContentPicker = function () {
            $scope.contentPickerOverlay = {
                multiPicker: false,
                view: "contentpicker",
                show: true,
                submit: function (model) {
                    if (angular.isArray(model.selection)) {
                        _.each(model.selection, function (item, i) {
                            $scope.contentItem = item;
                        });
                    }
                    else {
                        $scope.contentItem = model.selection;
                    }

                    $scope.model.entityId = $scope.contentItem.id;
                    getContentItemUrl().then(function (url) {
                        $scope.contentItem.url = url;
                        $scope.contentPickerOverlay.show = false;
                        $scope.contentPickerOverlay = null;
                    });
                },
                close: function () {
                    $scope.contentPickerOverlay.show = false;
                    $scope.contentPickerOverlay = null;
                }
            }
        };

        function getContentItemUrl() {
            return entityResource.getUrl($scope.contentItem.id, "Document");
        }

        function init() {
            if (!$scope.model.entityId && !$scope.enablePicker) {
                $scope.model.entityId = editorState.current.id;
            }
            else {
                $scope.loaded = true;
                return;
            }
            console.log($scope.model.entityId);
            entityResource.getById($scope.model.entityId, "Document").then(function (ent) {
                $scope.contentItem = ent;
                getContentItemUrl().then(function (url) {
                    $scope.contentItem.url = url;
                    $scope.loaded = true;
                });
            });
        }

        init();
    }
    angular.module("umbraco").controller("Umbraco.Overlays.RedirectUrlPickerController", RedirectUrlPickerController);
})();
