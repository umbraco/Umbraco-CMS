/**
 * @ngdoc controller
 * @name Umbraco.Editors.SchemaType.CreateController
 * @function
 *
 * @description
 * The controller for the schema type creation dialog
 */
function SchemaTypesCreateController($scope, $location, navigationService, schemaTypeResource, formHelper, appState, localizationService) {

    $scope.model = {
        folderName: "",
        creatingFolder: false
    };

    var node = $scope.dialogOptions.currentNode,
        localizeCreateFolder = localizationService.localize("defaultdialog_createFolder");

    $scope.showCreateFolder = function() {
        $scope.model.creatingFolder = true;
    }

    $scope.createContainer = function () {
        if (formHelper.submitForm({
            scope: $scope,
            formCtrl: this.createFolderForm,
            statusMessage: localizeCreateFolder
        })) {
            schemaTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "schematypes", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope });

                var section = appState.getSectionState("currentSection");

            }, function(err) {

               //TODO: Handle errors
            });
        };
    }

    $scope.createSchemaType = function() {
        $location.search('create', null);
        $location.path("/settings/schematypes/edit/" + node.id).search("create", "true");
        navigationService.hideMenu();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.SchemaTypes.CreateController", SchemaTypesCreateController);
