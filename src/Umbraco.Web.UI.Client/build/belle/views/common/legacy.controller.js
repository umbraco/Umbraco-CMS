angular.module("umbraco").controller("Umbraco.Common.LegacyController", 
	function($scope, $routeParams){
		$scope.legacyPath = decodeURI($routeParams.p);
	});