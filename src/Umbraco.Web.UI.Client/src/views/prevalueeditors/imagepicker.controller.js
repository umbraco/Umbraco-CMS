function imageFilePickerController($scope) {

    $scope.pick = function() {
        $scope.mediaPickerDialog = {};
        $scope.mediaPickerDialog.view = "mediapicker";
        $scope.mediaPickerDialog.show = true;

        $scope.mediaPickerDialog.submit = function(model) {
            $scope.model.value = model.selectedImages[0].image;
            $scope.mediaPickerDialog.show = false;
            $scope.mediaPickerDialog = null;
        };

        $scope.mediaPickerDialog.close = function(oldModel) {
            $scope.mediaPickerDialog.show = false;
            $scope.mediaPickerDialog = null;
        };
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.ImageFilePickerController", imageFilePickerController);
