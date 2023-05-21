/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberTypes.CreateController
 * @function
 *
 * @description
 * The controller for the member type creation dialog
 */
function MemberTypesCreateController($scope, $location, navigationService, memberTypeResource, formHelper, appState, localizationService) {

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
        if (formHelper.submitForm({
            scope: $scope,
            formCtrl: this.createFolderForm
        })) {
            memberTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "memberTypes", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope, formCtrl: this.createFolderForm });

            }, function(err) {
                formHelper.resetForm({ scope: $scope, formCtrl: this.createFolderForm, hasErrors: true });
               // TODO: Handle errors
            });
        };
    }

    $scope.createMemberType = function() {
        $location.search('create', null);
        $location.path("/" + section + "/memberTypes/edit/" + node.id).search("create", "true");
        navigationService.hideMenu();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.MemberTypes.CreateController", MemberTypesCreateController);
