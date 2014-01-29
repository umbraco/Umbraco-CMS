/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageCrop
* @restrict E
* @function
**/
angular.module("umbraco.directives")
	.directive('umbImageCrop', function ($timeout, localizationService, $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/directives/imaging/umb-image-crop.html',
				scope: {
					src: '=',
					width: '=',
					height: '=',
					crop: "="
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
						cropper:{},
						viewport:{},
						margin: 20,
						ratio: 1
					};

					scope.style = function () {
		                return { 
		                    'height': (parseInt(scope.height, 10) + 2 * scope.dimensions.margin) + 'px',
		                    'width': (parseInt(scope.width, 10) + 2 * scope.dimensions.margin) + 'px' 
		                };
		            };


					//elements
					var $viewport = element.find(".viewport"); 
					var $image = element.find("img");
					var $overlay = element.find(".overlay");
					var $container = element.find(".crop-container");

					//default constraints for drag n drop
					var constraints = {left: {max: 20, min: 20}, top: {max: 20, min: 20}, };
					
					var setDimensions = function(){
						scope.dimensions.image.width = $image.width();
						scope.dimensions.image.height = $image.height();

						scope.dimensions.viewport.width = $viewport.width();
						scope.dimensions.viewport.height = $viewport.height();

						scope.dimensions.cropper.width = scope.dimensions.viewport.width - 2 * scope.dimensions.margin;
						scope.dimensions.cropper.height = scope.dimensions.viewport.height - 2 * scope.dimensions.margin;
					};

					var setImageSize = function(width, height){
						$image.width(width); 
						$image.height(height);
						scope.dimensions.image.width = width;	
						scope.dimensions.image.height = height;
					};

					//when loading an image without any crop info, we center and fit it
					var fitImage = function(){
						fitToViewPort($image);
						centerImage($image);
						syncOverLay();
						setConstraints($image);
					};

					//utill for centering scaled image
					var centerImage = function(img) {
						var image_width   = img.width(),
						image_height  = img.height(),
						mask_width    = $viewport.width(),
						mask_height   = $viewport.height();
						
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
								scope.dimensions.cropper.width, 
								scope.dimensions.cropper.height, 
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

						syncOverLay();
					};

					//set constaints for cropping drag and drop
					var setConstraints = function(img){
						//do stuff
						var w = img.width(),
								h = img.height(),
								crop_width    = $viewport.width() - 2 * 20,
								crop_height   = $viewport.height() - 2 * 20;

						constraints.left.min = 20 + crop_width - w;
						constraints.top.min = 20 + crop_height - h;
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

					var calculateCropBox = function(){
						scope.crop.left = Math.abs($image[0].offsetLeft - scope.dimensions.margin) / scope.dimensions.image.width;
						scope.crop.top = Math.abs($image[0].offsetTop - scope.dimensions.margin) / scope.dimensions.image.height;

						scope.crop.right = 1 - Math.abs(scope.dimensions.cropper.width - (scope.dimensions.image.width - scope.crop.left)) / scope.dimensions.image.width;
						scope.crop.bottom = 1 - Math.abs(scope.dimensions.cropper.height - (scope.dimensions.image.height - scope.crop.top)) / scope.dimensions.image.height;
					};

					var calculatePosition = function(crop){

						var left = (crop.left * scope.dimensions.image.originalWidth);
						var top =  (crop.top * scope.dimensions.image.originalHeight);
						
						var cropped_width = scope.dimensions.image.originalWidth - left;
						var ratio =  cropped_width / scope.dimensions.image.originalWidth;

						scope.scale = ratio;
						resizeImageToScale($image, ratio);

						$image.css({
							"top": -top,
							"left": -left
						});

						syncOverLay();
					};



					var syncOverLay = function(){
						$overlay.height($image.height());
						$overlay.width($image.width());

						$overlay.css({
							"top": $image[0].offsetTop,
							"left": $image[0].offsetLeft
						});

						calculateCropBox();
					};

					//Drag and drop positioning, using jquery ui draggable
					var onStartDragPosition, top, left;
					$overlay.draggable({
						start: function(event, ui) {
							syncOverLay();
						},
						drag: function(event, ui) {


							if(ui.position.left <= constraints.left.max &&  ui.position.left >= constraints.left.min){
								$image.css({
									'left': ui.position.left
								});
							}

							if(ui.position.top <= constraints.top.max &&  ui.position.top >= constraints.top.min){
								$image.css({
									'top': ui.position.top
								});
							}
						},
						stop: function() {
							syncOverLay();
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

							if(scope.crop && scope.crop.top){
								calculatePosition(scope.crop);
							}else{
								scope.crop = {};
								fitImage();
							}
							
							scope.loaded = true;
						});
					});


					/// WATCHERS ////
					scope.$watch("scale", function(){
						if(scope.loaded && scope.scale !== scope.currentScale){
							resizeImageToScale($image, scope.scale);
							setConstraints($image);
						}
					});

					/// WATCHERS ////
					scope.$watch("crop", function(newVal, oldVal){
						if(scope.loaded && newVal !== oldVal){
							calculatePosition(scope.crop);
						}
					});
				}
			};
		});