/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.EmptyRecycleBinController
 * @function
 * 
 * @description
 * The controller for deleting media
 */
function MediaEmptyRecycleBinController($scope, mediaResource, treeService, navigationService, notificationsService, $route) {

    $scope.busy = false;

    $scope.performDelete = function() {

        //(used in the UI)
        $scope.busy = true;
        $scope.currentNode.loading = true;

        mediaResource.emptyRecycleBin($scope.currentNode.id).then(function (result) {

            $scope.busy = false;
            $scope.currentNode.loading = false;

            //show any notifications
            if (angular.isArray(result.notifications)) {
                for (var i = 0; i < result.notifications.length; i++) {
                    notificationsService.showNotification(result.notifications[i]);
                }
            }

            treeService.removeChildNodes($scope.currentNode);
            navigationService.hideMenu();

            //reload the current view
            $route.reload();
        });

    };

    $scope.cancel = function() {
        navigationService.hideDialog();
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Media.EmptyRecycleBinController", MediaEmptyRecycleBinController);
