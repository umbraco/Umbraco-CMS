/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageGravity
* @restrict E
* @function
* @description
**/
angular.module("umbraco.directives")
	.directive('umbImageGravity', function ($timeout, localizationService, $log) {
	    return {
				restrict: 'E',
				replace: true,
				templateUrl: 'views/components/imaging/umb-image-gravity.html',
				scope: {
					src: '=',
					center: "=",
                    onImageLoaded: "&",
                    onGravityChanged: "&"
				},
				link: function(scope, element, attrs) {

					//Internal values for keeping track of the dot and the size of the editor
					scope.dimensions = {
						width: 0,
						height: 0,
						left: 0,
						top: 0
					};

					scope.loaded = false;

					//elements
					var $viewport = element.find(".viewport");
					var $image = element.find("img");
                    var $overlay = element.find(".overlay");

				    scope.style = function () {
                        if (scope.dimensions.width <= 0) {
							setDimensions();
						}

						return {
							'top': scope.dimensions.top + 'px',
							'left': scope.dimensions.left + 'px'
						};
					};

					scope.setFocalPoint = function(event) {
					    scope.$emit("imageFocalPointStart");

					    var offsetX = event.offsetX - 10;
					    var offsetY = event.offsetY - 10;

					    calculateGravity(offsetX, offsetY);

					    gravityChanged();
					};

                    var setDimensions = function () {
                    if (scope.isCroppable) {
					        scope.dimensions.width = $image.width();
					        scope.dimensions.height = $image.height();
                            
					        if(scope.center){
							    scope.dimensions.left =  scope.center.left * scope.dimensions.width -10;
							    scope.dimensions.top =  scope.center.top * scope.dimensions.height -10;
						    }else{
							    scope.center = { left: 0.5, top: 0.5 };
                            }
                        }
					};

					var calculateGravity = function(offsetX, offsetY){
						scope.dimensions.left = offsetX;
						scope.dimensions.top =  offsetY;

						scope.center.left =  (scope.dimensions.left+10) / scope.dimensions.width;
						scope.center.top =  (scope.dimensions.top+10) / scope.dimensions.height;
					};

                    var gravityChanged = function () {
                        if (angular.isFunction(scope.onGravityChanged)) {
                            scope.onGravityChanged();
                        }
                    };

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
								var offsetX = $overlay[0].offsetLeft;
								var offsetY = $overlay[0].offsetTop;
								calculateGravity(offsetX, offsetY);
							});

							gravityChanged();
						}
					});

					//// INIT /////
					$image.load(function() {
					    $timeout(function() {
                        scope.isCroppable = true;
                        scope.hasDimensions = true;

                        if (scope.src) {
                            if (scope.src.endsWith(".svg")) {
                                scope.isCroppable = false;
                                scope.hasDimensions = false;
                            } else {
                                // From: https://stackoverflow.com/a/51789597/5018
                                var type = scope.src.substring(scope.src.indexOf("/") + 1, scope.src.indexOf(";base64"));
                                if (type.startsWith("svg")) {
                                    scope.isCroppable = false;
                                    scope.hasDimensions = false;
                                }
                            }
                        }

					        setDimensions();
					        scope.loaded = true;
					        if (angular.isFunction(scope.onImageLoaded)) {
                                scope.onImageLoaded(
                                    {
                                        "isCroppable": scope.isCroppable,
                                        "hasDimensions": scope.hasDimensions
                                    }
                                );
					        }
					    });
					});

					$(window).on('resize.umbImageGravity', function(){
                        scope.$apply(function(){
                            $timeout(function(){
                                setDimensions();
                            });
							// Make sure we can find the offset values for the overlay(dot) before calculating
							// fixes issue with resize event when printing the page (ex. hitting ctrl+p inside the rte)
							if($overlay.is(':visible')) {
								var offsetX = $overlay[0].offsetLeft;
								var offsetY = $overlay[0].offsetTop;
								calculateGravity(offsetX, offsetY);
							}
                        });
                    });

					scope.$on('$destroy', function() {
						$(window).off('.umbImageGravity');
					});

				}
			};
		});
