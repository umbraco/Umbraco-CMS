/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope, $routeParams, contentTypeResource, iconHelper) {

    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function(data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);

        // add focus to the first item
        if ($scope.allowedTypes.length > 0) {
            $scope.allowedTypes[0].hasFocus = true;
        }
    });
}

angular.module('umbraco').controller("Umbraco.Editors.Content.CreateController", contentCreateController);