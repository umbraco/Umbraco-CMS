/**
* @ngdoc directive
* @name umbraco.directives.directive:umbUploadDropzone
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
*
* @restrict E
**/

angular.module("umbraco.directives.html")
	.directive('umbUploadDropzone', function(){
		return {
			restrict: 'E',
			replace: true,
			templateUrl: 'views/directives/_obsolete/umb-upload-dropzone.html'
		};
	});
