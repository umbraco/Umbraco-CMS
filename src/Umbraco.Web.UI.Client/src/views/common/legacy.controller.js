/**
 * @ngdoc controller
 * @name Umbraco.LegacyController
 * @function
 * 
 * @description
 * A controller to control the legacy iframe injection
 * 
*/
function LegacyController($scope, $routeParams, $element) {
	$scope.legacyPath = decodeURIComponent($routeParams.url);
}

angular.module("umbraco").controller('Umbraco.LegacyController', LegacyController);