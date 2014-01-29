/**
* @ngdoc directive
* @name umbraco.directives.directive:umbCropsy
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbImageThumbnail', function ($timeout, localizationService, $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/directives/imaging/umb-image-thumbnail.html',
				
				scope: {
					src: '=',
					width: '=',
					height: '=',
					gravity: "=",
					crop: "="
				},
				
				link: function(scope, element, attrs) {
					scope.marginLeft =  0-Math.abs( scope.width * scope.gravity.left);
					scope.marginTop =  0-Math.abs( scope.width * scope.gravity.top);
				
					
				}
			};
		});