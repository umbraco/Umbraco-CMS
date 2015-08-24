/**
 * @ngdoc controller
 * @name Umbraco.Editors.MediaType.CreateController
 * @function
 *
 * @description
 * The controller for the media type creation dialog
 */
function mediaTypeCreateController($scope, $location, navigationService, mediaTypeResource, formHelper, appState) {

    $scope.model = {
        folderName: "",
        creatingFolder: false
    };

    var node = $scope.dialogOptions.currentNode;

    $scope.showCreateFolder = function() {
        $scope.model.creatingFolder = true;
    }

    $scope.createFolder = function () {
        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createFolderForm, statusMessage: "Creating folder..." })) {
            mediaTypeResource.createFolder(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "mediatype", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope });

                var section = appState.getSectionState("currentSection");

                $location.path("/" + section + "/mediatype/list/" + folderId);

            }, function(err) {

               //TODO: Handle errors
            });
        };
    }

    $scope.createMediaType = function() {
        $location.search('create', null);
        $location.path("/settings/mediatype/edit/" + node.id).search("create", true);
        navigationService.hideMenu();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.MediaType.CreateController", mediaTypeCreateController);
