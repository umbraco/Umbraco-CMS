/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.CreateController
 * @function
 * 
 * @description
 * The controller for the media creation dialog
 */
function mediaCreateController($scope, $routeParams, mediaTypeResource, iconHelper, authResource, mediaResource) {
    
    mediaTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    authResource.getCurrentUser().then(function (currentUser) {
        if (currentUser.allowedSections.indexOf("settings") > -1) {
            $scope.hasSettingsAccess = true;
            mediaResource.getById($scope.currentNode.id).then(function (data) {
                $scope.contentTypeId = data.contentTypeId;
            });
        }
    });
    
}

angular.module('umbraco').controller("Umbraco.Editors.Media.CreateController", mediaCreateController);