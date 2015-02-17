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
			templateUrl: 'views/directives/html/umb-upload-dropzone.html'
		};
	});