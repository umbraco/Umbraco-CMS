/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.CreateController
 * @function
 * 
 * @description
 * The controller for the media creation dialog
 */
function mediaCreateController($scope, $routeParams, mediaTypeResource, iconHelper) {
    
    mediaTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeThumbnails(data);
    });
    
}

angular.module('umbraco').controller("Umbraco.Editors.Media.CreateController", mediaCreateController);