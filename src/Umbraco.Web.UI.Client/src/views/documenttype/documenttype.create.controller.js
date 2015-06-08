/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.CreateController
 * @function
 * 
 * @description
 * The controller for the document type creation dialog
 */
function docTypeCreateController($scope, $routeParams, contentTypeResource, iconHelper) {

    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });
}

angular.module('umbraco').controller("Umbraco.Editors.DocumentType.CreateController", docTypeCreateController);