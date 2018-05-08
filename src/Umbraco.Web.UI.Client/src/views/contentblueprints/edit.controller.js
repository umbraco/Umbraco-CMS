/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentBlueprintEditController($scope, $routeParams, contentResource) {
  var excludedProps = ["_umb_urls", "_umb_releasedate", "_umb_expiredate", "_umb_template"];

  function getScaffold() {
    return contentResource.getScaffold(-1, $routeParams.doctype)
      .then(function (scaffold) {
        var lastTab = scaffold.tabs[scaffold.tabs.length - 1];
        lastTab.properties = _.filter(lastTab.properties,
          function(p) {
            return excludedProps.indexOf(p.alias) === -1;
          });
        scaffold.allowPreview = false;
        scaffold.allowedActions = ["A", "S", "C"];

        return scaffold;
      });
  }

  $scope.contentId = $routeParams.id;
  $scope.isNew = $routeParams.id === "-1";
  $scope.saveMethod = contentResource.saveBlueprint;
  $scope.getMethod = contentResource.getBlueprintById;
  $scope.getScaffoldMethod = getScaffold;
}

angular.module("umbraco").controller("Umbraco.Editors.ContentBlueprint.EditController", ContentBlueprintEditController);
