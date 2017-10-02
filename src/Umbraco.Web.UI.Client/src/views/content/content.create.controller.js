/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.CreateController
 * @function
 * 
 * @description
 * The controller for the content creation dialog
 */
function contentCreateController($scope,
  $routeParams,
  contentTypeResource,
  iconHelper,
  $location,
  navigationService,
  blueprintConfig) {

  function initialize() {
    contentTypeResource.getAllowedTypes($scope.currentNode.id).then(function (data) {
      $scope.allowedTypes = iconHelper.formatContentTypeIcons(data);
    });

    $scope.selectContentType = true;
    $scope.selectBlueprint = false;
    $scope.allowBlank = blueprintConfig.allowBlank;
  }

  function close() {
    navigationService.hideMenu();
  }

  function loadPageForEditing(path, search) {
    $location
      .path(path)
      .search(search);
    close();
  }

  function createBlank(docType) {
    var path = "/content/content/edit/" + $scope.currentNode.id;
    var search = "doctype=" + docType.alias + "&create=true";
    if ($scope.createWithName) {
      search += "&name=" + $scope.createWithName;
    }

    loadPageForEditing(path, search);
  }

  function createOrSelectBlueprintIfAny(docType) {
    var blueprintIds = _.keys(docType.blueprints || {});
    $scope.docType = docType;
    if (blueprintIds.length) {
        $scope.selectContentType = false;
        $scope.selectBlueprint = true;
    } else {
      createBlank(docType);
    }
  }

  function createFromBlueprint(blueprintId) {
    var path = "/content/content/edit/" + $scope.currentNode.id;
    var search = "doctype=" + $scope.docType.alias + "&create=true&blueprintId=" + blueprintId;
    if ($scope.createWithName) {
        search += "&name=" + $scope.createWithName;
    }

    loadPageForEditing(path, search);
  }

  $scope.createBlank = createBlank;
  $scope.createOrSelectBlueprintIfAny = createOrSelectBlueprintIfAny;
  $scope.createFromBlueprint = createFromBlueprint;

  initialize();
}

angular.module("umbraco").controller("Umbraco.Editors.Content.CreateController", contentCreateController);

angular.module("umbraco").value("blueprintConfig", {
    skipSelect: false,
    allowBlank: true
});