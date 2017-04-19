/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope, $routeParams, contentTypeResource, iconHelper, authResource, contentResource) {

    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    authResource.getCurrentUser().then(function (currentUser) {
        if (currentUser.allowedSections.indexOf("settings") > -1) {
            $scope.hasSettingsAccess = true;
            contentResource.getById($scope.currentNode.id).then(function(data) {
                $scope.contentTypeId = data.contentTypeId;
            });
        }
    });
}

angular.module('umbraco').controller("Umbraco.Editors.Content.CreateController", contentCreateController);