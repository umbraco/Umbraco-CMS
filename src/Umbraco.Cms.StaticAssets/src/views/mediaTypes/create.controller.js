/**
 * @ngdoc controller
 * @name Umbraco.Editors.MediaType.CreateController
 * @function
 *
 * @description
 * The controller for the media type creation dialog
 */
function MediaTypesCreateController($scope, $location, navigationService, mediaTypeResource, formHelper, appState, localizationService) {

    $scope.model = {
        folderName: "",
        creatingFolder: false
    };

    var node = $scope.currentNode;

    $scope.showCreateFolder = function() {
        $scope.model.creatingFolder = true;
    }

    $scope.createContainer = function () {
        if (formHelper.submitForm({
            scope: $scope,
            formCtrl: $scope.createFolderForm
        })) {
            mediaTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "mediaTypes", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm });

                var section = appState.getSectionState("currentSection");

            }, function (err) {
                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm, hasErrors: true });
                $scope.error = err;
            });
        };
    }

    $scope.createMediaType = function() {
        $location.search('create', null);
        $location.path("/settings/mediaTypes/edit/" + node.id).search("create", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
}

angular.module('umbraco').controller("Umbraco.Editors.MediaTypes.CreateController", MediaTypesCreateController);
