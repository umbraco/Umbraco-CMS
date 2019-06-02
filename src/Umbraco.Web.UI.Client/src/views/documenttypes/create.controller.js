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
        creatingFolder: false,
        creatingDoctypeCollection: false
    };

    var disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
    $scope.model.disableTemplates = disableTemplates;

    var node = $scope.currentNode;

    $scope.showCreateFolder = function () {
        $scope.model.creatingFolder = true;
    };

    $scope.showCreateDocTypeCollection = function () {
        $scope.model.creatingDoctypeCollection = true;
        $scope.model.collectionCreateTemplate = !$scope.model.disableTemplates;
        $scope.model.collectionItemCreateTemplate = !$scope.model.disableTemplates;
    };

    $scope.createContainer = function () {

        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createFolderForm })) {

            contentTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();

                var currPath = node.path ? node.path : "-1";

                navigationService.syncTree({
                    tree: "documenttypes",
                    path: currPath + "," + folderId,
                    forceReload: true,
                    activate: true
                });

                formHelper.resetForm({ scope: $scope });

                var section = appState.getSectionState("currentSection");

            }, function (err) {

                $scope.error = err;

            });
        }
    };

    $scope.createCollection = function () {

        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createDoctypeCollectionForm, statusMessage: "Creating Doctype Collection..." })) {

            // see if we can find matching icons
            var collectionIcon = "icon-folders", collectionItemIcon = "icon-document";
            iconHelper.getIcons().then(function (icons) {

                for (var i = 0; i < icons.length; i++) {
                    // for matching we'll require a full match for collection, partial match for item
                    if (icons[i].substring(5) == $scope.model.collectionName.toLowerCase()) {
                        collectionIcon = icons[i];
                    } else if (icons[i].substring(5).indexOf($scope.model.collectionItemName.toLowerCase()) > -1) {
                        collectionItemIcon = icons[i];
                    }
                }

                contentTypeResource.createCollection(node.id, $scope.model.collectionName, $scope.model.collectionCreateTemplate, $scope.model.collectionItemName, $scope.model.collectionItemCreateTemplate, collectionIcon, collectionItemIcon)
                    .then(function (collectionData) {

                        navigationService.hideMenu();
                        $location.search('create', null);
                        $location.search('notemplate', null);

                        formHelper.resetForm({
                            scope: $scope
                        });

                        var section = appState.getSectionState("currentSection");

                        // redirect to the item id
                        $location.path("/" + section + "/documenttypes/edit/" + collectionData.containerId);

                    }, function (err) {

                        $scope.error = err;

                        //show any notifications
                        if (angular.isArray(err.data.notifications)) {
                            for (var i = 0; i < err.data.notifications.length; i++) {
                                notificationsService.showNotification(err.data.notifications[i]);
                            }
                        }
                    });
            });
        }

    };

    // Disabling logic for creating document type with template if disableTemplates is set to true
    $scope.createDocType = function (cultureVariant) {
        $location.search("notemplate", null);
        $location.search("element", null);
        $location.search("variant", null);
        $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true");
        if (disableTemplates) {
            $location.search("notemplate", "true");
        }
        if (cultureVariant) {
            $location.search("variant", "true");
        }
        navigationService.hideMenu();
    };

    $scope.createElementType = function () {
        $location.search("notemplate", null);
        $location.search("variant", null);
        $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true").search("element", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
}

angular.module('umbraco').controller("Umbraco.Editors.DocumentTypes.CreateController", DocumentTypesCreateController);
