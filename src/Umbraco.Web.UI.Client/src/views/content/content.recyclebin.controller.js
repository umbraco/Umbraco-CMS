/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.RecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin for content
 * 
 */

function ContentRecycleBinController($scope, $routeParams, dataTypeResource) {

    //ensures the list view doesn't actually load until we query for the list view config
    // for the section
    $scope.listViewPath = null;

    $routeParams.id = "-20";
    dataTypeResource.getById(-95).then(function (result) {
        _.each(result.preValues, function (i) {
            $scope.model.config[i.key] = i.value;
        });
        $scope.listViewPath = 'views/propertyeditors/listview/listview.html';
    });

    $scope.model = { config: { entityType: $routeParams.section } };

}

angular.module('umbraco').controller("Umbraco.Editors.Content.RecycleBinController", ContentRecycleBinController);
