angular.module("umbraco")
    .controller("Umbraco.Editors.MediaPickerDetailsController",
        function ($scope) {

            var vm = this;

            vm.shouldShowUrl = shouldShowUrl;
            vm.focalPointChanged = focalPointChanged;

            console.log("$scope", $scope);
            console.log("$scope.model", $scope.model);

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

        });
