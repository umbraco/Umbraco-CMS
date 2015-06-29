/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPanel
* @restrict E
**/
angular.module("umbraco.directives.html")
	.directive('umbUploadDropzone', function(){
		return {
			restrict: 'E',
			replace: true,
			scope: {
				dropping: "=",
				files: "="
			},
			transclude: 'true',
			templateUrl: 'views/directives/html/umb-upload-dropzone.html'
		};
	});