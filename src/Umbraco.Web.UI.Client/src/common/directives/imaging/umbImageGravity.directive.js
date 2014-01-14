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
				templateUrl: 'views/directives/umb-image-gravity.html',
				scope: {
					src: '@',
					width: '@',
					height: '@',
					gravity: "="
				},
				link: function(scope, element, attrs) {
					
					scope.scale = 100;
					
					//if image is over this, we re-calculate the editors global ratio
					//this will not have an effect on the result, since that is returned in percentage
					scope.maxHeight = 500;
					scope.maxWidth = 600;
					
					scope.width = 400;
					scope.height = 320;

					scope.dimensions = {
						image: {},
						viewport:{},
						ratio: 1
					};

					scope.style = function () {
		                return { 
		                    'height': parseInt(scope.height, 10) + 'px',
		                    'width': parseInt(scope.width, 10) + 'px' 
		                };
		            };

					//elements
					var $viewport = element.find(".viewport"); 
					var $image = element.find("img");
					var $overlay = element.find(".overlay");
					var $container = element.find(".crop-container");

					var setImageSize = function(width, height){
						$image.width(width); 
						$image.height(height);
						
						$viewport.width(width); 
						$viewport.height(height);

						scope.dimensions.image.width = width;	
						scope.dimensions.image.height = height;
					};

					//when loading an image without any crop info, we center and fit it
					var fitImage = function(){
						fitToViewPort($image);
						centerImage($image);
					};

					//utill for centering scaled image
					var centerImage = function(img) {
						img.css({
							'position': 'absolute',
							'left': scope.dimensions.viewport.width / 2 - scope.dimensions.image.width / 2,
							'top': scope.dimensions.viewport.height / 2 - scope.dimensions.image.height / 2
						});
					};

					//utill for scaling image to fit viewport
					var fitToViewPort = function(img) {
						//returns size fitting the cropper	
						var size = calculateAspectRatioFit(
								scope.dimensions.image.width, 
								scope.dimensions.image.height, 
								scope.dimensions.viewport.width, 
								scope.dimensions.viewport.height, 
								true);

						//sets the image size and updates the scope
						setImageSize(size.width, size.height);

						scope.minScale = size.ratio;
						scope.maxScale = size.ratio * 3;
						scope.currentScale = scope.minScale;
						scope.scale = scope.currentScale;
					};

					var resizeImageToScale = function(img, ratio){
						//do stuff
						var size = calculateSizeToRatio(scope.dimensions.image.originalWidth, scope.dimensions.image.originalHeight, ratio);
						
						setImageSize(size.width, size.height);
						centerImage(img);
						scope.currentScale = scope.scale;
					};

					//utill for getting either min/max aspect ratio to scale image after
					var calculateAspectRatioFit = function(srcWidth, srcHeight, maxWidth, maxHeight, maximize) {
						var ratio = [maxWidth / srcWidth, maxHeight / srcHeight ];

						if(maximize){
							ratio = Math.max(ratio[0], ratio[1]);
						}else{
							ratio = Math.min(ratio[0], ratio[1]);
						}
							
						return { width:srcWidth*ratio, height:srcHeight*ratio, ratio: ratio};
					};
					
					//utill for scaling width / height given a ratio
					var calculateSizeToRatio= function(srcWidth, srcHeight, ratio) {
						return { width:srcWidth*ratio, height:srcHeight*ratio, ratio: ratio};
					};

					var calculateGravity = function(){
						scope.gravity.left = $overlay[0].offsetLeft + 10;
						scope.gravity.top =  $overlay[0].offsetTop + 10;
					};

					
					//Drag and drop positioning, using jquery ui draggable
					var onStartDragPosition, top, left;
					$overlay.draggable({
						stop: function() {
							calculateGravity();
						}
					});

					//// INIT /////
					$image.load(function(){
						$timeout(function(){
							$image.width("auto");
							$image.height("auto");

							scope.dimensions.image.originalWidth = $image.width();
							scope.dimensions.image.originalHeight = $image.height();

							setDimensions();

							fitImage();
							scope.loaded = true;
						});
					});


				}
			};
		});