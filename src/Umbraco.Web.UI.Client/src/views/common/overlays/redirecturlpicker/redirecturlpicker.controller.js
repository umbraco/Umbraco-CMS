(function () {
    "use strict";
    function validateUrl() {
        var pathRegex = new RegExp("^([\/]{1})[^\/].*(?<!\/)$")
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, ctrl) {
                ctrl.$parsers.unshift(function (viewValue) {

                    viewValue = viewValue.trim();

                    if (viewValue.length == 0) { return viewValue; }
                    if (viewValue.endsWith("/")) {
                        viewValue = viewValue.slice(0, -1);
                    }
                    if (pathRegex.test(viewValue)) {
                        ctrl.$setValidity('path', true);
                    }
                    else {
                        ctrl.$setValidity('path', false);
                        return undefined;
                    }

                    if (viewValue.indexOf(".") < 0) {
                        ctrl.$setValidity('dotPath', true);
                    }
                    else {
                        ctrl.$setValidity('dotPath', false);                        
                        return undefined;
                    }
                    return viewValue;
                });            
            }
        };
    }
    function RedirectUrlPickerController($scope, entityResource, editorState) {

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
    angular.module("umbraco").directive('validateUrl', validateUrl);
})();
