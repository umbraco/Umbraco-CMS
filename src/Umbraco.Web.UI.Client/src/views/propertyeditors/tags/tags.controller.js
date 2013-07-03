angular.module("umbraco")
.controller("Umbraco.Editors.TagsController",
	function($rootScope, $scope, $log, tagsResource, scriptLoader) {	
		
		scriptLoader.load(
			[
			'views/propertyeditors/tags/bootstrap-tags.custom.js',
			'css!views/propertyeditors/tags/bootstrap-tags.custom.css'
			]).then(function(){

			// Get data from tagsFactory
			$scope.tags = tagsResource.getTags("group");

			// Initialize bootstrap-tags.js script
			var tags = $('#' + $scope.model.alias + "_tags").tags({
				tagClass: 'label-inverse'
			});

			$.each($scope.tags, function(index, tag) {
				tags.addTag(tag.label);
			});
		});
	}
);