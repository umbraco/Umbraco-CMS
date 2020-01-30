function iconPreValsController($scope, editorService) {

    if (!$scope.model.value) {
        $scope.model.value = "icon-list";
    }

    $scope.openIconPicker = function () {
        var iconPicker = {
            icon: $scope.model.value.split(' ')[0],
            color: $scope.model.value.split(' ')[1],
            submit: function (model) {
                if (model.icon) {
                    if (model.color) {
                        $scope.model.value = model.icon + " " + model.color;
                    } else {
                        $scope.model.value = model.icon;
                    }
                    $scope.iconForm.$setDirty();
                }
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.iconPicker(iconPicker);
    };
}


angular.module("umbraco").controller("Umbraco.PrevalueEditors.IconPickerController", iconPreValsController);
