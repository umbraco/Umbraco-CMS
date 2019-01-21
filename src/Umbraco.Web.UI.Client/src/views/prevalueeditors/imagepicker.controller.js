function imageFilePickerController($scope) {

    $scope.add = function() {
        $scope.mediaPickerOverlay = {
            view: "mediapicker",
            disableFolderSelect: true,
            onlyImages: true,
            show: true,
            submit: function (model) {
                $scope.model.value = model.selection[0].image;
                $scope.mediaPickerOverlay.show = false;
                $scope.mediaPickerOverlay = null;
            },
            close: function () {
                $scope.mediaPickerOverlay.show = false;
                $scope.mediaPickerOverlay = null;
            }
        };
    };

    $scope.remove = function () {
        $scope.model.value = null;
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.ImageFilePickerController", imageFilePickerController);
