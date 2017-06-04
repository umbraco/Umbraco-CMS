/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, contentResource) {

  $scope.contentId = $routeParams.id;
  $scope.saveMethod = contentResource.save;
  $scope.getMethod = contentResource.getById;
  $scope.page = $routeParams.page;
  $scope.createOptions = null;
  if ($routeParams.create && $routeParams.doctype) {
    $scope.createOptions = {
      docType: $routeParams.doctype
    }
  }
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
