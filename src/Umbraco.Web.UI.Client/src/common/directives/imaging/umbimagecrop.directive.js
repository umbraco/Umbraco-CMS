/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageCrop
* @restrict E
* @function
**/
angular.module("umbraco.directives")
	.directive('umbImageCrop', 
		function ($timeout, localizationService, cropperHelper,  $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/directives/imaging/umb-image-crop.html',
				scope: {
					src: '=',
					width: '@',
					height: '@',
					crop: "="
				},

				link: function(scope, element, attrs) {
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
						scale: {
							min: 0.3,
							max: 3,
							current: 1
						}
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
					var setDimensions = function(originalImage){
						originalImage.width("auto");
						originalImage.height("auto");

						scope.dimensions.image.originalWidth = originalImage.width();
						scope.dimensions.image.originalHeight = originalImage.height();

						scope.dimensions.image.width = originalImage.width();
						scope.dimensions.image.height = originalImage.height();
						scope.dimensions.image.left = originalImage[0].offsetLeft;
						scope.dimensions.image.top = originalImage[0].offsetTop;

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
						scope.dimensions.image.left = $image[0].offsetLeft;
						scope.dimensions.image.top = $image[0].offsetTop;
					};

					//when loading an image without any crop info, we center and fit it
					var fitImage = function(){
						fitToViewPort($image);
						
						cropperHelper.centerInsideViewPort($image, $viewport);

						syncOverLay();
						setConstraints($image);
					};

					//utill for scaling image to fit viewport
					var fitToViewPort = function(img) {
						
						//returns size fitting the cropper	
						var size = cropperHelper.calculateAspectRatioFit(
								scope.dimensions.image.width, 
								scope.dimensions.image.height, 
								scope.dimensions.cropper.width, 
								scope.dimensions.cropper.height, 
								true);

						//sets the image size and updates the scope
						setImageSize(size.width, size.height);

						scope.dimensions.scale.min = size.ratio;
						scope.dimensions.scale.max = size.ratio * 3;
						scope.dimensions.scale.current = size.ratio;
					};


					var resizeImageToScale = function(img, ratio){
						//do stuff
						var size = cropperHelper.calculateSizeToRatio(scope.dimensions.image.originalWidth, scope.dimensions.image.originalHeight, ratio);
						setImageSize(size.width, size.height);
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

						var original = cropperHelper.calculateAspectRatioFit(
								scope.dimensions.image.originalWidth, 
								scope.dimensions.image.originalHeight, 
								scope.dimensions.cropper.width, 
								scope.dimensions.cropper.height, 
								true);

						scope.dimensions.scale.current = ratio;

						//min max based on original width/height
						scope.dimensions.scale.min = original.ratio;
						scope.dimensions.scale.max = 2;

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
						stop: function(event, ui) {

							scope.dimensions.image.left = $image[0].offsetLeft;
							scope.dimensions.image.top = $image[0].offsetTop;

							
							syncOverLay();
						}
					});


					
					var init = function(image){

						scope.loaded = false;

						//set dimensions on image, viewport, cropper etc
						setDimensions(image);

						//if we have a crop already position the image
						if(scope.crop && scope.crop.top){
							calculatePosition(scope.crop);
						}else{
							//if not, reset it and fit the image to the viewport
							scope.crop = {};
							fitImage();
						}

						scope.loaded = true;
					};

					//// INIT /////
					$image.load(function(){
						$timeout(function(){
							init($image);
						});
					});


					/// WATCHERS ////
					scope.$watchCollection('[width, height]', function(newValues, oldValues){
							//we have to reinit the whole thing if
							//one of the external params changes
							if(newValues !== oldValues){
								setDimensions($image);
							}
					});

					
					scope.$watch("dimensions.scale.current", function(){
						if(scope.loaded){
							resizeImageToScale($image, scope.dimensions.scale.current);
							setConstraints($image);
						}
					});
				}
			};
		});