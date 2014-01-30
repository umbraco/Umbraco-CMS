/**
* @ngdoc directive
* @name umbraco.directives.directive:umbCropsy
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbImageGravity', function ($timeout, localizationService, $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/directives/imaging/umb-image-gravity.html',
				scope: {
					src: '=',
					width: "=",
					height: "=",
					gravity: "="
				},
				link: function(scope, element, attrs) {
					
					scope.dimensions = {
						width: 0,
						height: 0,
						left: 0,
						top: 0
					};

					//elements
					var $viewport = element.find(".viewport"); 
					var $image = element.find("img");
					var $overlay = element.find(".overlay");
					
					scope.style = function () {
						return {
							'top': scope.dimensions.top + 'px',
							'left': scope.dimensions.left + 'px' 
						};
					};

					var setDimensions = function(){
						scope.dimensions.width = $image.width();
						scope.dimensions.height = $image.height();

						if(scope.gravity){
							scope.dimensions.left =  scope.gravity.left * scope.dimensions.width -10;
							scope.dimensions.top =  scope.gravity.top * scope.dimensions.height -10;
						}
					};

					var calculateGravity = function(){
						scope.dimensions.left = $overlay[0].offsetLeft + 10;
						scope.dimensions.top =  $overlay[0].offsetTop + 10;

						scope.gravity.left =  scope.gravity.left / scope.dimensions.width;
						scope.gravity.top =  scope.gravity.top / scope.dimensions.height;
					};

					
					//Drag and drop positioning, using jquery ui draggable
					$overlay.draggable({
						stop: function() {
							calculateGravity();
						}
					});

					//// INIT /////
					$image.load(function(){
						$timeout(function(){
							setDimensions();
						});
					});
				}
			};
		});