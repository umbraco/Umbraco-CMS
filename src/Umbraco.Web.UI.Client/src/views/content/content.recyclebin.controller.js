/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.RecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin for content
 * 
 */

function ContentRecycleBinController($scope, $routeParams, dataTypeResource, navigationService, localizationService) {

    //ensures the list view doesn't actually load until we query for the list view config
    // for the section
    $scope.page = {};
    $scope.page.name = "Recycle Bin";
    $scope.page.nameLocked = true;

    $scope.listViewPath = null;

    $routeParams.id = "-20";
    dataTypeResource.getById(-95).then(function (result) {
        _.each(result.preValues, function (i) {
            $scope.model.config[i.key] = i.value;
        });
        $scope.listViewPath = 'views/propertyeditors/listview/listview.html';
    });

    $scope.model = { config: { entityType: $routeParams.section } };

    // sync tree node
    navigationService.syncTree({ tree: "content", path: ["-1", $routeParams.id], forceReload: false });

    localizePageName();

    function localizePageName() {

       var pageName = "general_recycleBin";

       localizationService.localize(pageName).then(function(value) {
          $scope.page.name = value;
       });

    }
}

angular.module('umbraco').controller("Umbraco.Editors.Content.RecycleBinController", ContentRecycleBinController);
