angular.module('umbraco')
.controller("Umbraco.Editors.ContentCreateController",
	function ($scope, $routeParams, contentTypeResource, iconHelper) {

	    contentTypeResource.getAllowedTypes($scope.currentNode.id)
	        .then(function (data) {
	            _.each(data, function(item) {
	                item.icon = iconHelper.convertFromLegacyIcon(item.icon);
	            });
	            $scope.allowedTypes = data;
	        });
	});