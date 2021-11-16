/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentBlueprintEditController($scope, $routeParams, contentResource) {
    function getScaffold() {
        return contentResource.getScaffold(-1, $routeParams.doctype)
            .then(function (scaffold) {
                return initialize(scaffold);
            });
    }

    function getBlueprintById(id) {
        return contentResource.getBlueprintById(id).then(function (blueprint) {
            return initialize(blueprint);
        });
    }

    function initialize(content) {
        if (content.apps && content.apps.length) {
            var contentApp = _.find(content.apps, function (app) {
                return app.alias === "umbContent";
            });
            if (contentApp) {
                content.apps = [contentApp];
            } else {
                content.apps = [];
            }
        }
        content.allowPreview = false;
        content.allowedActions = ["A", "S", "C"];
        return content;
    }

    $scope.contentId = $routeParams.id;
    $scope.isNew = $routeParams.id === "-1";
    $scope.saveMethod = contentResource.saveBlueprint;
    $scope.getMethod = getBlueprintById;
    $scope.getScaffoldMethod = getScaffold;

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
        $scope.isNew = $routeParams.id === "-1";
        $scope.contentId = $routeParams.id;
    });

}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.EditController", ContentBlueprintEditController);
