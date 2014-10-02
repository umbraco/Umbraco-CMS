/**
 * @ngdoc controller
 * @name Umbraco.Dashboard.RecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin dashboards
 * 
 */

function RecycleBinController($scope, $routeParams, dataTypeResource) {

    //ensures the list view doesn't actually load until we query for the list view config
    // for the section
    $scope.listViewPath = null;

    if ($routeParams.section) {

		if ($routeParams.section === "content") {
		    $routeParams.id = "-20";
		    dataTypeResource.getById(-95).then(function(result) {
		        _.each(result.preValues, function(i) {
		            $scope.model.config[i.key] = i.value;
		        });
		        $scope.listViewPath = 'views/propertyeditors/listview/listview.html';
		    });
		}
		else if ($routeParams.section === "media") {
		    $routeParams.id = "-21";
		    dataTypeResource.getById(-96).then(function (result) {
		        _.each(result.preValues, function (i) {
		            $scope.model.config[i.key] = i.value;
		        });		        
		        $scope.listViewPath = 'views/propertyeditors/listview/listview.html';
		    });
		}

		$scope.model = { config: { entityType: $routeParams.section } };
	}
}

angular.module('umbraco').controller("Umbraco.Dashboard.RecycleBinController", RecycleBinController);
