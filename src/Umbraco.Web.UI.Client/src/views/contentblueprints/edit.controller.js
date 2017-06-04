/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentBlueprintEditController($scope, $routeParams) {
  $scope.contentId = $routeParams.id;  
}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.EditController", ContentEditController);
