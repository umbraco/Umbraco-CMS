/**
 * @ngdoc controller
 * @name Umbraco.Dashboard.RecycleBinController
 * @function
 * 
 * @description
 * Controls the recycle bin dashboards
 * 
 */

function RecycleBinController($scope, $routeParams) {
	$routeParams.id = "-20"; // media = "-21"
	$scope.model = { config: { entityType: "content" } };
}

angular.module('umbraco').controller("Umbraco.Dashboard.RecycleBinController", RecycleBinController);
