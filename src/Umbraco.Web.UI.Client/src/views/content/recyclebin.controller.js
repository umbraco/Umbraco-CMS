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
	if ($routeParams.section) {

		if ($routeParams.section === "content") {
			$routeParams.id = "-20";
		}
		else if ($routeParams.section === "media") {
			$routeParams.id = "-21";
		}

		$scope.model = { config: { entityType: $routeParams.section } };
	}
}

angular.module('umbraco').controller("Umbraco.Dashboard.RecycleBinController", RecycleBinController);
