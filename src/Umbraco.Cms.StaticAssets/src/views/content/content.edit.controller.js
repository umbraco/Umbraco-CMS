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
    function scaffoldInfiniteEmpty() {
        return contentResource.getScaffold($scope.model.parentId, $scope.model.documentTypeAlias);
    }
    function scaffoldBlueprint() {
        return contentResource.getBlueprintScaffold($routeParams.id, $routeParams.blueprintId);
    }

    $scope.contentId = infiniteMode ? $scope.model.id : $routeParams.id;
    $scope.saveMethod = contentResource.save;
    $scope.getMethod = contentResource.getById;
    $scope.getScaffoldMethod = $routeParams.blueprintId ? scaffoldBlueprint : infiniteMode ? scaffoldInfiniteEmpty : scaffoldEmpty;
    $scope.page = $routeParams.page;
    $scope.isNew = infiniteMode ? $scope.model.create : $routeParams.create;
    //load the default culture selected in the main tree if any
    $scope.culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
    $scope.segment = $routeParams.csegment ? $routeParams.csegment : null;

    //Bind to $routeUpdate which will execute anytime a location changes but the route is not triggered.
    //This is so we can listen to changes on the cculture parameter since that will not cause a route change
    //and then we can pass in the updated culture to the editor.
    //This will also execute when we are redirecting from creating an item to a newly created item since that
    //will not cause a route change and so we can update the isNew and contentId flags accordingly.
    $scope.$on('$routeUpdate', function (event, next) {
        $scope.culture = next.params.cculture ? next.params.cculture : $routeParams.mculture;
        $scope.segment = next.params.csegment ? next.params.csegment : null;
        $scope.isNew = next.params.create === "true";
        $scope.contentId = infiniteMode ? $scope.model.id : $routeParams.id;
    });
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
