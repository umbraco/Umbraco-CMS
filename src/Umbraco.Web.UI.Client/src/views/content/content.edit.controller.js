/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, contentResource) {



  function scaffoldEmpty() {
    return contentResource.getScaffold($routeParams.id, $routeParams.doctype);
  }
  function scaffoldBlueprint() {
      return contentResource.getBlueprintScaffold($routeParams.id, $routeParams.blueprintId);
  }

  $scope.contentId = $routeParams.id;
  $scope.saveMethod = contentResource.save;
  $scope.getMethod = contentResource.getById;
  $scope.getScaffoldMethod = $routeParams.blueprintId ? scaffoldBlueprint : scaffoldEmpty;
  $scope.page = $routeParams.page;
  $scope.isNew = $routeParams.create;
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
