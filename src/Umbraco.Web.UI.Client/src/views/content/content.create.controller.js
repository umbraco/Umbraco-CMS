/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope, $routeParams, contentTypeResource, iconHelper, $location, navigationService) {
    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function (data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    $scope.selectContentType = true;
    $scope.selectBlueprint = false;

    $scope.createBlank = function (docType) {
        $location
            .path("/content/content/edit/" + $scope.currentNode.id)
            .search("doctype=" + docType.alias + "&create=true");
        navigationService.hideMenu();
    }

    $scope.createOrSelectBlueprintIfAny = function (docType) {
        if (docType.blueprints && _.keys(docType.blueprints).length) {
            $scope.docType = docType;
            $scope.selectContentType = false;
            $scope.selectBlueprint = true;
        } else {
            $scope.createBlank(docType);
        }
    }

    $scope.createFromBlueprint = function (blueprintId) {
        $location
            .path("/content/content/edit/" + $scope.currentNode.id)
            .search(
            "doctype=" + $scope.docType.alias +
            "&create=true" +
            "&blueprintId=" + blueprintId
            );
        navigationService.hideMenu();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.Content.CreateController", contentCreateController);