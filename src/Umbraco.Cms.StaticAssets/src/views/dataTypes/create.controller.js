/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.CreateController
 * @function
 *
 * @description
 * The controller for the data type creation dialog
 */
function DataTypeCreateController($scope, $location, navigationService, dataTypeResource, formHelper, appState) {

    $scope.model = {
        folderName: "",
        creatingFolder: false
    };

    var node = $scope.currentNode;
    var section = appState.getSectionState("currentSection");

    $scope.showCreateFolder = function() {
        $scope.model.creatingFolder = true;
    }

    $scope.createContainer = function () {
        if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createFolderForm })) {
            dataTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "datatypes", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm });

            }, function(err) {

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm, hasErrors: true });
               // TODO: Handle errors
            });
        };
    }

    $scope.createDataType = function() {
        $location.search('create', null);
        $location.path("/" + section + "/dataTypes/edit/" + node.id).search("create", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };

}

angular.module('umbraco').controller("Umbraco.Editors.DataType.CreateController", DataTypeCreateController);
