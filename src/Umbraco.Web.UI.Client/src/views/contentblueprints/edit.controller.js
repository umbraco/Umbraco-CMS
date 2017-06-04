/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentBlueprintEditController($scope, $routeParams, contentResource) {
  $scope.contentId = $routeParams.id;  
  $scope.saveMethod = contentResource.saveBlueprint;
  $scope.getMethod = contentResource.getBlueprintById;
}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.EditController", ContentBlueprintEditController);
