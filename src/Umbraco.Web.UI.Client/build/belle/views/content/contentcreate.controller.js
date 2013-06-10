angular.module('umbraco')
.controller("Umbraco.Editors.ContentCreateController",
	function ($scope, $routeParams,contentTypeResource) {
	$scope.allowedTypes  = contentTypeResource.getAllowedTypes($scope.currentNode.id);
});