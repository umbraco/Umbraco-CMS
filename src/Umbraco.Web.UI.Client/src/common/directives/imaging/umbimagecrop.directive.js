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
					crop: "=",
					center: "=",
					maxSize: '@'
				},

				link: function(scope, element, attrs) {
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


					//live rendering of viewport and image styles
					scope.style = function () {
						return { 
							'height': (parseInt(scope.dimensions.viewport.height, 10)) + 'px',
							'width': (parseInt(scope.dimensions.viewport.width, 10)) + 'px' 
						};
					};
				

					//elements
					var $viewport = element.find(".viewport"); 
					var $image = element.find("img");
					var $overlay = element.find(".overlay");
					var $container = element.find(".crop-container");

					//default constraints for drag n drop
					var constraints = {left: {max: scope.dimensions.margin, min: scope.dimensions.margin}, top: {max: scope.dimensions.margin, min: scope.dimensions.margin}, };
					scope.constraints = constraints;


					//set constaints for cropping drag and drop
					var setConstraints = function(){
						constraints.left.min = scope.dimensions.margin + scope.dimensions.cropper.width - scope.dimensions.image.width;
						constraints.top.min = scope.dimensions.margin + scope.dimensions.cropper.height - scope.dimensions.image.height;
					};


					var setDimensions = function(originalImage){	
						originalImage.width("auto");
						originalImage.height("auto");

						var image = {};
						image.originalWidth = originalImage.width();
						image.originalHeight = originalImage.height();

						image.width = image.originalWidth;
						image.height = image.originalHeight;
						image.left = originalImage[0].offsetLeft;
						image.top = originalImage[0].offsetTop;

						scope.dimensions.image = image;

						//unscaled editor size
						//var viewPortW =  $viewport.width();
						//var viewPortH =  $viewport.height();
						var _viewPortW =  parseInt(scope.width, 10);
						var _viewPortH =  parseInt(scope.height, 10);

						//if we set a constraint we will scale it down if needed
						if(scope.maxSize){	
							var ratioCalculation = cropperHelper.scaleToMaxSize(
									_viewPortW, 
									_viewPortH,
									scope.maxSize);

							//so if we have a max size, override the thumb sizes
							_viewPortW = ratioCalculation.width;
							_viewPortH = ratioCalculation.height;	
						}

						scope.dimensions.viewport.width = _viewPortW + 2 * scope.dimensions.margin;
						scope.dimensions.viewport.height = _viewPortH + 2 * scope.dimensions.margin;
						scope.dimensions.cropper.width = _viewPortW; // scope.dimensions.viewport.width - 2 * scope.dimensions.margin;
						scope.dimensions.cropper.height = _viewPortH; //  scope.dimensions.viewport.height - 2 * scope.dimensions.margin;
					};


					//when loading an image without any crop info, we center and fit it
					var resizeImageToEditor = function(){
						//returns size fitting the cropper	
						var size = cropperHelper.calculateAspectRatioFit(
								scope.dimensions.image.width, 
								scope.dimensions.image.height, 
								scope.dimensions.cropper.width, 
								scope.dimensions.cropper.height, 
								true);

						//sets the image size and updates the scope
						scope.dimensions.image.width = size.width;
						scope.dimensions.image.height = size.height;

						//calculate the best suited ratios
						scope.dimensions.scale.min = size.ratio;
						scope.dimensions.scale.max = 2;
						scope.dimensions.scale.current = size.ratio;

						//center the image
						var position = cropperHelper.centerInsideViewPort(scope.dimensions.image, scope.dimensions.cropper);
						scope.dimensions.top = position.top;
						scope.dimensions.left = position.left;

						setConstraints();
					};

					//resize to a given ratio
					var resizeImageToScale = function(ratio){
						//do stuff
						var size = cropperHelper.calculateSizeToRatio(scope.dimensions.image.originalWidth, scope.dimensions.image.originalHeight, ratio);
						scope.dimensions.image.width = size.width;
						scope.dimensions.image.height = size.height;

						setConstraints();
						validatePosition(scope.dimensions.image.left, scope.dimensions.image.top);
					};

					//resize the image to a predefined crop coordinate
					var resizeImageToCrop = function(){
						scope.dimensions.image = cropperHelper.convertToStyle(
												scope.crop, 
												{width: scope.dimensions.image.originalWidth, height: scope.dimensions.image.originalHeight},
												scope.dimensions.cropper,
												scope.dimensions.margin);

						var ratioCalculation = cropperHelper.calculateAspectRatioFit(
								scope.dimensions.image.originalWidth, 
								scope.dimensions.image.originalHeight, 
								scope.dimensions.cropper.width, 
								scope.dimensions.cropper.height, 
								true);

						scope.dimensions.scale.current = scope.dimensions.image.ratio;

						//min max based on original width/height
						scope.dimensions.scale.min = ratioCalculation.ratio;
						scope.dimensions.scale.max = 2;
					};



					var validatePosition = function(left, top){
						if(left > constraints.left.max)
						{
							left = constraints.left.max; 
						}

						if(left <= constraints.left.min){
							left = constraints.left.min;
						}

						if(top > constraints.top.max)
						{
							top = constraints.top.max; 
						}
						if(top <= constraints.top.min){
							top = constraints.top.min;
						}

						if(scope.dimensions.image.left !== left){
							scope.dimensions.image.left = left;
						}	
						
						if(scope.dimensions.image.top !== top){
							scope.dimensions.image.top = top;
						}
					};	


					//sets scope.crop to the recalculated % based crop	
					var calculateCropBox = function(){
						scope.crop = cropperHelper.pixelsToCoordinates(scope.dimensions.image, scope.dimensions.cropper.width, scope.dimensions.cropper.height, scope.dimensions.margin);	
					};


					//Drag and drop positioning, using jquery ui draggable
					var onStartDragPosition, top, left;
					$overlay.draggable({
						drag: function(event, ui) {
							scope.$apply(function(){
								validatePosition(ui.position.left, ui.position.top);
							});
						},
						stop: function(event, ui){
							scope.$apply(function(){
								//make sure that every validates one more time...
								validatePosition(ui.position.left, ui.position.top);

								calculateCropBox();
								scope.dimensions.image.rnd = Math.random();
							});
						}
					});
					


					var init = function(image){
						scope.loaded = false;

						//set dimensions on image, viewport, cropper etc
						setDimensions(image);

						//if we have a crop already position the image
						if(scope.crop){
							resizeImageToCrop();
						}else{
							resizeImageToEditor();
						}

						//sets constaints for the cropper
						setConstraints();
						scope.loaded = true;
					};


					/// WATCHERS ////
					scope.$watchCollection('[width, height]', function(newValues, oldValues){
							//we have to reinit the whole thing if
							//one of the external params changes
							if(newValues !== oldValues){
								setDimensions($image);
								setConstraints();
							}
					});

					var throttledResizing = _.throttle(function(){
						resizeImageToScale(scope.dimensions.scale.current);
						calculateCropBox();	
					}, 100);

					
					//happens when we change the scale
					scope.$watch("dimensions.scale.current", function(){
						if(scope.loaded){
							throttledResizing();
						}
					});

					//ie hack
					if(window.navigator.userAgent.indexOf("MSIE ")){
						var ranger = element.find("input");
						ranger.bind("change",function(){
							scope.$apply(function(){
								scope.dimensions.scale.current = ranger.val();
							});
						});	
					}
					
					//// INIT /////
					$image.load(function(){
						$timeout(function(){
							init($image);
						});
					});
				}
			};
		});