angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.GridPrevalueEditorController.Dialogs.EditConfig",
    function ($scope, $http) {

            $scope.renderModel = {};
            $scope.renderModel.name = $scope.dialogOptions.name;
            $scope.renderModel.config = $scope.dialogOptions.config;

            $scope.saveAndClose = function(isValid){
                if(isValid){
                    $scope.submit($scope.renderModel.config);
                }
            };

    });
