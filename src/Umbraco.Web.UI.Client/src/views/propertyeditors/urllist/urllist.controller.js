angular.module('umbraco').controller("Umbraco.Editors.UrlListController",
	function($rootScope, $scope, $filter) {

	    $scope.renderModel = _.map($scope.model.value.split(","), function(item) {
	        return {
	            url: item,
	            urlTarget : ($scope.config && $scope.config.target) ? $scope.config.target : "_blank" 
	        };
	    });

	});