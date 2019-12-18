function imageFilePickerController($scope, editorService) {

    $scope.add = function() {
        var mediaPickerOptions = {
            view: "mediapicker",
            multiPicker: false,
            disableFolderSelect: true,
            onlyImages: true,
            submit: function (model) {
                $scope.model.value = model.selection[0].image;

                editorService.close();
            },
            close: function () {
                editorService.close();
            } 
        };
        editorService.mediaPicker(mediaPickerOptions);
    };

    $scope.remove = function () {
        $scope.model.value = null;
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.ImageFilePickerController", imageFilePickerController);
