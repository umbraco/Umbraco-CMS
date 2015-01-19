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
					center: "="
				},
				link: function(scope, element, attrs) {
					
					//Internal values for keeping track of the dot and the size of the editor
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
						if(scope.dimensions.width <= 0){
							setDimensions();
						}

						return {
							'top': scope.dimensions.top + 'px',
							'left': scope.dimensions.left + 'px' 
						};
					};

					var setDimensions = function(){
						scope.dimensions.width = $image.width();
						scope.dimensions.height = $image.height();

						if(scope.center){
							scope.dimensions.left =  scope.center.left * scope.dimensions.width -10;
							scope.dimensions.top =  scope.center.top * scope.dimensions.height -10;
						}else{
							scope.center = { left: 0.5, top: 0.5 };
						}
					};	

					var calculateGravity = function(){
						scope.dimensions.left = $overlay[0].offsetLeft;
						scope.dimensions.top =  $overlay[0].offsetTop;

						scope.center.left =  (scope.dimensions.left+10) / scope.dimensions.width;
						scope.center.top =  (scope.dimensions.top+10) / scope.dimensions.height;
					};
					
					var lazyEndEvent = _.debounce(function(){
						scope.$apply(function(){
							scope.$emit("imageFocalPointStop");
						});
					}, 2000);
					

					//Drag and drop positioning, using jquery ui draggable
					//TODO ensure that the point doesnt go outside the box
					$overlay.draggable({
						containment: "parent",
						start: function(){
							scope.$apply(function(){
								scope.$emit("imageFocalPointStart");
							});
						},
						stop: function() {
							scope.$apply(function(){
								calculateGravity();
							});

							lazyEndEvent();
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