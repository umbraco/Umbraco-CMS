/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.CreateController
 * @function
 *
 * @description
 * The controller for the doc type creation dialog
 */
function DocumentTypesCreateController($scope, $location, navigationService, contentTypeResource, formHelper, appState, notificationsService) {

    $scope.model = {
        folderName: "",
        creatingFolder: false,        
    };

    var node = $scope.dialogOptions.currentNode;

    $scope.showCreateFolder = function() {
        $scope.model.creatingFolder = true;
    }

    $scope.createContainer = function () {
        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createFolderForm, statusMessage: "Creating folder..." })) {
            contentTypeResource.createContainer(node.id, $scope.model.folderName).then(function (folderId) {

                navigationService.hideMenu();
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "documenttypes", path: currPath + "," + folderId, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope });

                var section = appState.getSectionState("currentSection"); 
                
            }, function(err) {

                $scope.error = err;

                //show any notifications
                if (angular.isArray(err.data.notifications)) {
                    for (var i = 0; i < err.data.notifications.length; i++) {
                        notificationsService.showNotification(err.data.notifications[i]);
                    }
                }
            });
        };
    }

    $scope.createDocType = function() {
        $location.search('create', null);
        $location.search('notemplate', null);
        $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true");
        navigationService.hideMenu();
    }

    $scope.createComponent = function() {
      $location.search('create', null);
      $location.search('notemplate', null);
      $location.path("/settings/documenttypes/edit/" + node.id).search("create", "true").search("notemplate", "true");
      navigationService.hideMenu();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.DocumentTypes.CreateController", DocumentTypesCreateController);
