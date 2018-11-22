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
  
  var mainCulture = $routeParams.mculture ? $routeParams.mculture : null;

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

  function createBlank(docType) {
      $location
          .path("/content/content/edit/" + $scope.currentNode.id)
          .search("doctype", docType.alias)
          .search("create", "true")
          /* when we create a new node we want to make sure it uses the same 
          language as what is selected in the tree */
          .search("cculture", mainCulture);
    close();
  }

  function createOrSelectBlueprintIfAny(docType) {
    var blueprintIds = _.keys(docType.blueprints || {});
    $scope.docType = docType;
    if (blueprintIds.length) {
      if (blueprintConfig.skipSelect) {
        createFromBlueprint(blueprintIds[0]);
      } else {
        $scope.selectContentType = false;
        $scope.selectBlueprint = true;
      }
    } else {
      createBlank(docType);
    }
  }

  function createFromBlueprint(blueprintId) {
    $location
      .path("/content/content/edit/" + $scope.currentNode.id)
          .search("doctype", $scope.docType.alias)
          .search("create", "true")
          .search("blueprintId", blueprintId);
    close();
  }

  $scope.closeDialog = function(showMenu) {
    navigationService.hideDialog(showMenu);
  };

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
