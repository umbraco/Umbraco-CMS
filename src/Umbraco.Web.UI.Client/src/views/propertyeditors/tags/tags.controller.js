angular.module("umbraco")
.controller("Umbraco.Editors.TagsController",
    function ($rootScope, $scope, $log, assetsService) {
		
		assetsService.loadJs(
			'views/propertyeditors/tags/bootstrap-tags.custom.js'
			).then(function(){

			//// Get data from tagsFactory
			//$scope.tags = tagsResource.getTags("group");
			$scope.tags = [];

			// Initialize bootstrap-tags.js script
			var tags = $('#' + $scope.model.alias + "_tags").tags({
				tagClass: 'label-inverse'
			});

			$.each($scope.tags, function(index, tag) {
				tags.addTag(tag.label);
			});
		});

		assetsService.loadCss('views/propertyeditors/tags/bootstrap-tags.custom.css');
	}
);