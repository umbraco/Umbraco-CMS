angular.module('umbraco')
.controller("Umbraco.Editors.ContentCreateController",
	function ($scope, $routeParams,contentTypeResource) {

	    contentTypeResource.getAllowedTypes($scope.currentNode.id)
	        .then(function(data) {
	            $scope.allowedTypes = data;
	        });
	});