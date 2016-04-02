/**
 * @ngdoc controller
 * @name Umbraco.Editors.UserTypes.CreateController
 * @function
 * 
 * @description
 * The controller for the member creation dialog
 */
function userTypesCreateController($scope, $routeParams, memberTypeResource, iconHelper) {
    
    memberTypeResource.getTypes($scope.currentNode.id).then(function (data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });
    
}

angular.module('umbraco').controller("Umbraco.Editors.UserTypes.CreateController", userTypesCreateController);