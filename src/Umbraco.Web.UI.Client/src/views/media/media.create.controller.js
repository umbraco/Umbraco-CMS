/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.CreateController
 * @function
 * 
 * @description
 * The controller for the media creation dialog
 */
function mediaCreateController($scope, $location, mediaTypeResource, iconHelper, navigationService, authResource, mediaResource) {

    function initialize() {
        $scope.allowedTypes = null;
        mediaTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
            $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
        });

        if ($scope.currentNode.id > -1) {
            authResource.getCurrentUser().then(function(currentUser) {
                if (currentUser.allowedSections.indexOf("settings") > -1) {
                    $scope.hasSettingsAccess = true;
                    mediaResource.getById($scope.currentNode.id).then(function (data) {
                        $scope.mediaTypeId = data.contentType.id;
                    });
                }
            });
        }
    }

    $scope.createMediaItem = function(docType) {
        $location.path("/media/media/edit/" + $scope.currentNode.id).search("doctype", docType.alias).search("create", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        navigationService.hideMenu();
    };

    $scope.closeDialog = function () {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };

    // the current node changes behind the scenes when the context menu is clicked without closing 
    // the default menu first, so we must watch the current node and re-initialize accordingly
    var unbindModelWatcher = $scope.$watch("currentNode", initialize);
    $scope.$on('$destroy', function () {
        unbindModelWatcher();
    });
}

angular.module('umbraco').controller("Umbraco.Editors.Media.CreateController", mediaCreateController);
