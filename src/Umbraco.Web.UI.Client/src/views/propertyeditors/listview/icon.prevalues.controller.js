function iconPreValsController($scope, editorService) {

    if (!$scope.model.value) {
        $scope.model.value = "icon-list";
    }

    let valueArray = $scope.model.value.split(' ');
    $scope.icon = valueArray[0];
    $scope.color = valueArray[1];

    $scope.openIconPicker = function () {
        var iconPicker = {
            icon: $scope.icon,
            color: $scope.color,
            size: "medium",
            submit: function (model) {
                if (model.icon) {
                    if (model.color) {
                        $scope.model.value = model.icon + " " + model.color;
                        $scope.color = model.color;
                    } else {
                        $scope.model.value = model.icon;
                    }                    

                    $scope.icon = model.icon;
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
