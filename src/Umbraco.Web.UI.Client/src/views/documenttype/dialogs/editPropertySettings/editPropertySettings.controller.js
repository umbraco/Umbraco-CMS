/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditPropertySettingsController($scope, contentTypeResource) {

    $scope.changePropertyEditor = function(property){

        $scope.dialogModel = {};
        $scope.dialogModel.title = "Change property type";
        $scope.dialogModel.dataTypes = $scope.model.dataTypes;
        $scope.dialogModel.view = "views/documentType/dialogs/property.html";
        $scope.showDialog = true;

        $scope.dialogModel.submit = function(dt){
            contentTypeResource.getPropertyTypeScaffold(dt.id)
                .then(function(pt){

                    // save data to property
                    property.config = pt.config;
                    property.editor = pt.editor;
                    property.view = pt.view;
                    property.dataType = dt;

                    // close dialog
                    $scope.dialogModel = null;
                    $scope.showDialog = false;

                });
        };

        $scope.dialogModel.close = function(model){
            $scope.showDialog = false;
            $scope.dialogModel = null;
        };
    };

    $scope.editDataType = function(property) {

        $scope.dialogModel = {};
        $scope.dialogModel.title = "Edit data type";
        $scope.dialogModel.property = property;
        $scope.dialogModel.view = "views/documentType/dialogs/editDataType/editDataType.html";
        $scope.showDialog = true;

        $scope.dialogModel.submit = function(dt){
            alert("submit from edit data type");
            $scope.showDialog = false;
            $scope.dialogModel = null;
        };

        $scope.dialogModel.close = function(model){
            $scope.showDialog = false;
            $scope.dialogModel = null;
        };

    }




}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditPropertySettingsController", EditPropertySettingsController);
