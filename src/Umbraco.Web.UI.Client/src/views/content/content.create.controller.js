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

    $scope.goToAction = function (docType) {
        docType.blueprints = [
          {id:1, name:"A blue print"},
          {id:2, name:"A red print"}
      ];

        if (docType.blueprints && docType.blueprints.length) {
          // Show dialog
        } else {
          $location.path( "/content/edit/" +
            $scope.currentNode.id +
            "?doctype=" +
            docType.alias +
            "&create=true"
          );
        }
        navigationService.hideNavigation();
    }

    $scope.createLinkAttributes = function(docType) {
      return linkAttributes;
    }
}

angular.module('umbraco').controller("Umbraco.Editors.Content.CreateController", contentCreateController);