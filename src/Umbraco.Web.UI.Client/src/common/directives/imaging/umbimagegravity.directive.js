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
					
					//elements
					var $viewport = element.find(".viewport"); 
					var $image = element.find("img");
					var $overlay = element.find(".overlay");
					

					var setDimensions = function(){
						scope.imagewidth = $image.width();
						scope.imageheight = $image.height();
					};

					var setImageSize = function(width, height){
						$image.width(width); 
						$image.height(height);
						
						$viewport.width(width); 
						$viewport.height(height);
					};

					var fitImage = function(){
						fitToViewPort($image);
						centerImage($image);
						$log.log("centered and fitted");
					};

					//utill for centering scaled image
					var centerImage = function(img) {
						img.css({
							'position': 'absolute',
							'left': scope.width / 2 - scope.imageWidth / 2,
							'top': scope.height / 2 - scope.imageHeight / 2
						});
					};

					//utill for scaling image to fit viewport
					var fitToViewPort = function(img) {
						//returns size fitting the cropper	
						var size = calculateAspectRatioFit(
								scope.imageWidth, 
								scope.imageHeight, 
								scope.width, 
								scope.height, 
								false);

						//sets the image size and updates the scope
						setImageSize(size.width, size.height);
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

							setDimensions();
							fitImage();

							scope.loaded = true;
						});
					});


				}
			};
		});