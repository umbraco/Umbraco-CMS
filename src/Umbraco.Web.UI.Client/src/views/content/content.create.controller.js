/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope, $routeParams, contentTypeResource, iconHelper, $location) {
    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function (data) {
        $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    $scope.selectContentType = true;
    $scope.selectBlueprint = false;

    $scope.createOrSelectBlueprintIfAny = function (docType) {
        docType.blueprints =
            {
                "1": "A blue print",
                "2": "A red print"
            };

        if (docType.blueprints && _.keys(docType.blueprints).length) {
            $scope.docType = docType;
            $scope.selectContentType = false;
            $scope.selectBlueprint = true;
        } else {
            $location
                .path("/content/content/edit/" + $scope.currentNode.id)
                .search("doctype=" + docType.alias + "&create=true");
            $scope.close();
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
        $scope.close();
    }
}

angular.module('umbraco').controller("Umbraco.Editors.Content.CreateController", contentCreateController);