/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams) {

  $scope.contentId = $routeParams.id;
  $scope.page = $routeParams.page;
  $scope.createOptions = null;
  if ($routeParams.create && $routeParams.doctype) {
    $scope.createOptions = {
      docType: $routeParams.doctype
    }
  }
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
