/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.CreateController
 * @function
 *
 * @description
 * The controller for the doc type creation dialog
 */
function DocumentTypesCreateController($scope, $location, navigationService, contentTypeResource, formHelper, appState, notificationsService, localizationService, iconHelper) {

    $scope.model = {
        allowCreateFolder: $scope.currentNode.parentId === null || $scope.currentNode.nodeType === "container",
        folderName: "",
        creatingFolder: false
    };

    var disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
    $scope.model.disableTemplates = disableTemplates;

    var node = $scope.currentNode;

    $scope.showCreateFolder = function () {
        $scope.model.creatingFolder = true;
    };

    $scope.createContainer = function () {

        if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createFolderForm })) {

            contentTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();

                var currPath = node.path ? node.path : "-1";

                navigationService.syncTree({
                    tree: "documenttypes",
                    path: currPath + "," + folderId,
                    forceReload: true,
                    activate: true
                });

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm });

                var section = appState.getSectionState("currentSection");

            }, function (err) {

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createFolderForm, hasErrors: true });
                $scope.error = err;

            });
        }
    };   

    // Disabling logic for creating document type with template if disableTemplates is set to true
    if (!disableTemplates) {
        $scope.createDocType = function () {
            $location.search('create', null);
            $location.search('notemplate', null);
            $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true");
            navigationService.hideMenu();
        };
    }

    $scope.createComponent = function () {
        $location.search('create', null);
        $location.search('notemplate', null);
        $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true").search("notemplate", "true");
        navigationService.hideMenu();
    };

    $scope.createElement = function () {
        $location.search('create', null);
        $location.search('notemplate', null);
        $location.search('iselement', null);
        $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true").search("notemplate", "true").search("iselement", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
}

angular.module('umbraco').controller("Umbraco.Editors.DocumentTypes.CreateController", DocumentTypesCreateController);
