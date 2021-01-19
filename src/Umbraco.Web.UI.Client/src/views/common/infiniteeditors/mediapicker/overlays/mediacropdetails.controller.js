﻿angular.module("umbraco")
    .controller("Umbraco.Editors.MediaCropDetailsController",
        function ($scope) {

            var vm = this;

            vm.submit = submit;
            vm.close = close;
            vm.hasCrops = cropSet() === true;

            vm.disableFocalPoint = false;
            if(typeof $scope.model.disableFocalPoint === "boolean") {
                vm.disableFocalPoint = $scope.model.disableFocalPoint
            }
            else {
                vm.disableFocalPoint = ($scope.model.disableFocalPoint !== undefined && $scope.model.disableFocalPoint !== "0") ? true : false;
            }

            if (!$scope.model.target.coordinates && !$scope.model.target.focalPoint) {
                $scope.model.target.focalPoint = { left: .5, top: .5 };
            }

            vm.shouldShowUrl = shouldShowUrl;
            vm.focalPointChanged = focalPointChanged;

            if (!$scope.model.target.image) {
                $scope.model.target.image = $scope.model.target.url;
            }

            function shouldShowUrl() {
                if (!$scope.model.target) {
                    return false;
                }
                if ($scope.model.target.id) {
                    return false;
                }
                if ($scope.model.target.url && $scope.model.target.url.toLower().indexOf("blob:") === 0) {
                    return false;
                }
                return true;
            }

            /**
             * Called when the umbImageGravity component updates the focal point value
             * @param {any} left
             * @param {any} top
             */
            function focalPointChanged(left, top) {
                // update the model focalpoint value
                $scope.model.target.focalPoint = {
                    left: left,
                    top: top
                };
            }

            function submit() {
                if ($scope.model && $scope.model.submit) {
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if ($scope.model && $scope.model.close) {
                    $scope.model.close($scope.model);
                }
            }

            function cropSet() {
                var model = $scope.model;
                return (model.cropSize || {}).width !== undefined && (model.cropSize || {}).height !== undefined;
            }
        });
