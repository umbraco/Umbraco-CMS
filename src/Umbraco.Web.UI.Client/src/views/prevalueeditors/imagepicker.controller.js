function imageFilePickerController($scope, dialogService, mediaHelper) {

    $scope.pick = function() {
        dialogService.mediaPicker({
            multiPicker: false,
            callback: function(data) {
                 $scope.model.value = mediaHelper.resolveFile(data, false);
            }
        });
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.ImageFilePickerController",imageFilePickerController);
