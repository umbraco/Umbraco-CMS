/**
* @ngdoc directive
* @name umbraco.directives.directive:umbUploadDropzone
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
