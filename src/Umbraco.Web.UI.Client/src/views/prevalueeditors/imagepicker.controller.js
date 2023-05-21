function imageFilePickerController($scope, editorService) {
    var vm = this;
    vm.model = $scope.model;

    vm.add = add;
    vm.remove = remove;

    function add() {
        var mediaPickerOptions = {
            view: "mediapicker",
            multiPicker: false,
            disableFolderSelect: true,
            onlyImages: true,
            submit: function (model) {
                vm.model.value = model.selection[0].image;

                editorService.close();
            },
            close: function () {
                editorService.close();
            } 
        };
        editorService.mediaPicker(mediaPickerOptions);
    };

    function remove() {
        vm.model.value = null;
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.ImageFilePickerController", imageFilePickerController);
