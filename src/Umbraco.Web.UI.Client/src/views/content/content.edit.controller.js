/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $rootScope, $routeParams, contentResource) {

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
    //load the default culture selected in the main tree if any
    $scope.culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;

    //Bind to $routeUpdate which will execute anytime a location changes but the route is not triggered.
    //This is so we can listen to changes on the cculture parameter since that will not cause a route change
    // and then we can pass in the updated culture to the editor
    $scope.$on('$routeUpdate', function (event, next) {
        $scope.culture = next.params.cculture ? next.params.cculture : $routeParams.mculture;
    });
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
