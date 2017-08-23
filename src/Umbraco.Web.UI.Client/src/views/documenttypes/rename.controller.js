angular.module("umbraco")
    .controller("Umbraco.Editors.DocumentTypes.RenameController",
    [
        "$scope",
        "contentTypeResource",
        "navigationService",
        "notificationsService",
        "localizationService",
        function (scope, contentTypeResource, navigationService, notificationsService, localizationService) {
            scope.strings = {
                
            }

            scope.model = {
                folderName: scope.currentNode.name 
            }

            scope.renameContainer = function () {

                contentTypeResource.renameContainer(scope.currentNode.id, scope.model.folderName)
                    .then(function() {
                        
                        notificationsService.showNotification({
                            type: 0,
                            header: "Renamed",
                            message: scope.currentNode.name + " was renamed to " + scope.model.folderName
                        });

                        navigationService.hideMenu();

                    }, function (err) {
                        scope.error = err;

                        if (angular.isArray(err.data.notifications)) {
                            for (var i = 0; i < err.data.notifications.length; i++) {
                                notificationsService.showNotification(err.data.notifications[i]);
                            }
                        }
                    });

            }

        }
    ]);