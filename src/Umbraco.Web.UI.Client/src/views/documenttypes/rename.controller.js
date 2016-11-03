angular.module("umbraco")
    .controller("Umbraco.Editors.DocumentTypes.RenameController",
    [
        "$scope",
        "contentTypeResource",
        "navigationService",
        "notificationsService",
        "localizationService",
        function (scope, contentTypeResource, navigationService, notificationsService, localizationService) {
            var notificationHeader;

            function reportSuccessAndClose() {
                var lastComma = scope.currentNode.path.lastIndexOf(","),
                    path = lastComma === -1
                        ? scope.currentNode.path
                        : scope.currentNode.path.substring(0, lastComma - 1);

                navigationService.syncTree({
                    tree: "documenttypes",
                    path: path,
                    forceReload: true,
                    activate: true
                });

                localizationService.localize(
                    "renamecontainer_folderWasRenamed",
                    [scope.currentNode.name, scope.model.folderName])
                    .then(function (msg) {
                        notificationsService.showNotification({
                            type: 0,
                            header: notificationHeader,
                            message: msg
                        });
                    });

                navigationService.hideMenu();
            }


            localizationService.localize("renamecontainer_renamed")
                .then(function(s) { notificationHeader = s; });

            scope.model = {
                folderName: scope.currentNode.name 
            }

            scope.renameContainer = function () {

                contentTypeResource.renameContainer(scope.currentNode.id, scope.model.folderName)
                    .then(reportSuccessAndClose, function (err) {
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