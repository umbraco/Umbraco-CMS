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

    function createDocType( config ) {

        $location.search("create", null);
        $location.search("notemplate", null);
        $location.search("iscomposition", null);
        $location.search("iselement", null);
        $location.search("icon", null);

        var icon = null;

        if (config.icon) {
            var icon = config.icon;
            if (config.color) {
                icon += " " + config.color;
            }
        }

        $location
            .path("/settings/documenttypes/edit/" + node.id)
            .search("create", "true")
            .search("notemplate", config.notemplate ? "true" : null)
            .search("iscomposition", config.iscomposition ? "true" : null)
            .search("iselement", config.iselement ? "true" : null)
            .search("icon", icon);

        navigationService.hideMenu();
    }


    // Disabling logic for creating document type with template if disableTemplates is set to true
    if (!disableTemplates) {
        $scope.createDocType = function (icon, color) {
            createDocType({ icon: icon, color: color });
        };
    }

    $scope.createComponent = function (icon, color) {
        createDocType({ notemplate: true, icon:icon, color:color });
    };

    $scope.createComposition = function (icon, color) {
        createDocType({ iscomposition: true, iselement: true, icon: icon, color: color });
    };

    $scope.createElement = function (icon, color) {
        createDocType({ iselement: true, icon: icon, color: color });
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
}

angular.module('umbraco').controller("Umbraco.Editors.DocumentTypes.CreateController", DocumentTypesCreateController);
