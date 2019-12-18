/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageThumbnail
* @restrict E
* @function
* @description
**/
angular.module("umbraco.directives")
	.directive('umbImageThumbnail',
		function ($timeout, localizationService, cropperHelper, $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/components/imaging/umb-image-thumbnail.html',

				scope: {
					src: '=',
					width: '@',
					height: '@',
					center: "=",
					crop: "=",
					maxSize: '@'
				},

				link: function(scope, element, attrs) {
					//// INIT /////
					var $image = element.find("img");
					scope.loaded = false;

                    $image.on("load", function() {
                        $timeout(function () {
                            $image.width("auto");
                            $image.height("auto");

                            scope.image = {};
                            scope.image.width = $image[0].width;
                            scope.image.height = $image[0].height;

                            //we force a lower thumbnail size to fit the max size
                            //we do not compare to the image dimensions, but the thumbs
                            if (scope.maxSize) {
                                var ratioCalculation = cropperHelper.calculateAspectRatioFit(
                                    scope.width,
                                    scope.height,
                                    scope.maxSize,
                                    scope.maxSize,
                                    false);

                                //so if we have a max size, override the thumb sizes
                                scope.width = ratioCalculation.width;
                                scope.height = ratioCalculation.height;
                            }

                            setPreviewStyle();
                            scope.loaded = true;
                        });
                    })

					/// WATCHERS ////
					scope.$watchCollection('[crop, center]', function(newValues, oldValues){
							//we have to reinit the whole thing if
							//one of the external params changes
							setPreviewStyle();
					});

					scope.$watch("center", function(){
						setPreviewStyle();
					}, true);

					function setPreviewStyle(){
						if(scope.crop && scope.image){
							scope.preview = cropperHelper.convertToStyle(
												scope.crop,
												scope.image,
												{width: scope.width, height: scope.height},
												0);
						}else if(scope.image){

							//returns size fitting the cropper
							var p = cropperHelper.calculateAspectRatioFit(
									scope.image.width,
									scope.image.height,
									scope.width,
									scope.height,
									true);


							if(scope.center){
								var xy = cropperHelper.alignToCoordinates(p, scope.center, {width: scope.width, height: scope.height});
								p.top = xy.top;
								p.left = xy.left;
							}else{

							}

							p.position = "absolute";
							scope.preview = p;
						}
					}
				}
			};
		});
