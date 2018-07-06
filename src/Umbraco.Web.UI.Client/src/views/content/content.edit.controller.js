/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, contentResource) {

    var infiniteMode = $scope.model && $scope.model.infiniteMode;

    function scaffoldEmpty() {
        return contentResource.getScaffold($routeParams.id, $routeParams.doctype);
    }
    function scaffoldBlueprint() {
        return contentResource.getBlueprintScaffold($routeParams.id, $routeParams.blueprintId);
    }

    $scope.contentId = infiniteMode ? $scope.model.id : $routeParams.id;
    $scope.saveMethod = contentResource.save;
    $scope.getMethod = contentResource.getById;
    $scope.getScaffoldMethod = $routeParams.blueprintId ? scaffoldBlueprint : scaffoldEmpty;
    $scope.page = $routeParams.page;
    $scope.isNew = infiniteMode ? $scope.model.create : $routeParams.create;
    $scope.culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture; //load the default culture selected in the main tree if any
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
