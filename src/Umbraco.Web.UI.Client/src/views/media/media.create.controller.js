/**
 * @ngdoc controller
 * @name Umbraco.Editors.Media.CreateController
 * @function
 * 
 * @description
 * The controller for the media creation dialog
 */
function mediaCreateController($scope, $routeParams, $location, mediaTypeResource, iconHelper, navigationService) {
    
    mediaTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    $scope.createMediaItem = function(docType) {
        $location.path("/media/media/edit/" + $scope.currentNode.id).search("doctype", docType.alias).search("create", "true");
        navigationService.hideMenu();
    };

    $scope.close = function() {
        const showMenu = true;
        navigationService.hideDialog(showMenu);
    };
    
}

angular.module('umbraco').controller("Umbraco.Editors.Media.CreateController", mediaCreateController);