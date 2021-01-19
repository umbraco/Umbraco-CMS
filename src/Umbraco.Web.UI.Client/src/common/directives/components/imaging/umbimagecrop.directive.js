/**
* @ngdoc directive
* @name umbraco.directives.directive:umbImageCrop
* @restrict E
* @function
**/
angular.module("umbraco.directives")
    .directive('umbImageCrop',
        function ($timeout, cropperHelper) {
            return {
                restrict: 'E',
                replace: true,
                templateUrl: 'views/components/imaging/umb-image-crop.html',
                scope: {
                    src: '=',
                    width: '@',
                    height: '@',
                    crop: "=",
                    center: "=",
                    maxSize: '@'
                },

                link: function (scope, element, attrs) {

                    let sliderRef = null;

                    scope.width = 400;
                    scope.height = 320;

                    scope.dimensions = {
                        image: {},
                        cropper: {},
                        viewport: {},
                        margin: 20,
                        scale: {
                            min: 0,
                            max: 3,
                            current: 1
                        }
                    };

                    scope.sliderOptions = {
                        "start": scope.dimensions.scale.current,
                        "step": 0.001,
                        "tooltips": [false],
                        "format": {
                            to: function (value) {
                                return parseFloat(parseFloat(value).toFixed(3)); //Math.round(value);
                            },
                            from: function (value) {
                                return parseFloat(parseFloat(value).toFixed(3)); //Math.round(value);
                            }
                        },
                        "range": {
                            "min": scope.dimensions.scale.min,
                            "max": scope.dimensions.scale.max
                        }
                    };

                    scope.setup = function (slider) {
                        sliderRef = slider;

                        // Set slider handle position
                        sliderRef.noUiSlider.set(scope.dimensions.scale.current);

                        // Update slider range min/max
                        sliderRef.noUiSlider.updateOptions({
                            "range": {
                                "min": scope.dimensions.scale.min,
                                "max": scope.dimensions.scale.max
                            }
                        });
                    };

                    scope.slide = function (values) {
                        if (values) {
                            scope.dimensions.scale.current = parseFloat(values);
                        }
                    };

                    scope.change = function (values) {
                        if (values) {
                            scope.dimensions.scale.current = parseFloat(values);
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
                    var constraints = { left: { max: scope.dimensions.margin, min: scope.dimensions.margin }, top: { max: scope.dimensions.margin, min: scope.dimensions.margin } };
                    scope.constraints = constraints;


                    //set constaints for cropping drag and drop
                    var setConstraints = function () {
                        constraints.left.min = scope.dimensions.margin + scope.dimensions.cropper.width - scope.dimensions.image.width;
                        constraints.top.min = scope.dimensions.margin + scope.dimensions.cropper.height - scope.dimensions.image.height;
                    };

                    var setDimensions = function (originalImage) {
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
                        var _viewPortW = parseInt(scope.width, 10);
                        var _viewPortH = parseInt(scope.height, 10);

                        //if we set a constraint we will scale it down if needed
                        if (scope.maxSize) {
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

                    //resize to a given ratio
                    var resizeImageToScale = function (ratio) {
                        //do stuff
                        var size = cropperHelper.calculateSizeToRatio(scope.dimensions.image.originalWidth, scope.dimensions.image.originalHeight, ratio);
                        scope.dimensions.image.width = size.width;
                        scope.dimensions.image.height = size.height;

                        setConstraints();
                        validatePosition(scope.dimensions.image.left, scope.dimensions.image.top);
                    };

                    //resize the image to a predefined crop coordinate
                    var resizeImageToCrop = function () {
                        scope.dimensions.image = cropperHelper.convertToStyle(
                            scope.crop,
                            { width: scope.dimensions.image.originalWidth, height: scope.dimensions.image.originalHeight },
                            scope.dimensions.cropper,
                            scope.dimensions.margin);

                        var ratioCalculation = cropperHelper.calculateAspectRatioFit(
                            scope.dimensions.image.originalWidth,
                            scope.dimensions.image.originalHeight,
                            scope.dimensions.cropper.width,
                            scope.dimensions.cropper.height,
                            true);

                        scope.dimensions.scale.current = scope.dimensions.image.ratio;

                        // Update min and max based on original width/height
                        scope.dimensions.scale.min = ratioCalculation.ratio;
                        scope.dimensions.scale.max = 2;
                    };

                    var validatePosition = function (left, top) {
                        if (left > constraints.left.max) {
                            left = constraints.left.max;
                        }

                        if (left <= constraints.left.min) {
                            left = constraints.left.min;
                        }

                        if (top > constraints.top.max) {
                            top = constraints.top.max;
                        }
                        if (top <= constraints.top.min) {
                            top = constraints.top.min;
                        }

                        if (scope.dimensions.image.left !== left) {
                            scope.dimensions.image.left = left;
                        }

                        if (scope.dimensions.image.top !== top) {
                            scope.dimensions.image.top = top;
                        }
                    };


                    //sets scope.crop to the recalculated % based crop
                    var calculateCropBox = function () {
                        scope.crop = cropperHelper.pixelsToCoordinates(scope.dimensions.image, scope.dimensions.cropper.width, scope.dimensions.cropper.height, scope.dimensions.margin);
                    };


                    //Drag and drop positioning, using jquery ui draggable
                    var onStartDragPosition, top, left;
                    $overlay.draggable({
                        drag: function (event, ui) {
                            scope.$apply(function () {
                                validatePosition(ui.position.left, ui.position.top);
                            });
                        },
                        stop: function (event, ui) {
                            scope.$apply(function () {
                                //make sure that every validates one more time...
                                validatePosition(ui.position.left, ui.position.top);

                                calculateCropBox();
                                scope.dimensions.image.rnd = Math.random();
                            });
                        }
                    });

                    var init = function (image) {
                        scope.loaded = false;

                        //set dimensions on image, viewport, cropper etc
                        setDimensions(image);

                        //create a default crop if we haven't got one already
                        var createDefaultCrop = !scope.crop;
                        if (createDefaultCrop) {
                            calculateCropBox();
                        }

                        resizeImageToCrop();

                        //if we're creating a new crop, make sure to zoom out fully
                        if (createDefaultCrop) {
                            scope.dimensions.scale.current = scope.dimensions.scale.min;
                            resizeImageToScale(scope.dimensions.scale.min);

                            if (scope.center) {
                                // Move image to focal point if set
                                // Repeating a few calls here, but logic is too difficult to follow elsewhere
                                var x1 = Math.min(
                                    Math.max(
                                        scope.center.left * scope.dimensions.image.width - scope.dimensions.cropper.width / 2,
                                        0
                                    ),
                                    scope.dimensions.image.width - scope.dimensions.cropper.width
                                );
                                var y1 = Math.min(
                                    Math.max(
                                        scope.center.top * scope.dimensions.image.height - scope.dimensions.cropper.height / 2,
                                        0
                                    ),
                                    scope.dimensions.image.height - scope.dimensions.cropper.height
                                );
                                scope.dimensions.image.left = x1;
                                scope.dimensions.image.top = y1;
                                calculateCropBox();
                                resizeImageToCrop();
                            }
                        }

                        //sets constaints for the cropper
                        setConstraints();
                        scope.loaded = true;
                    };


                    // Watchers
                    scope.$watchCollection('[width, height]', function (newValues, oldValues) {
                        // We have to reinit the whole thing if
                        // one of the external params changes
                        if (newValues !== oldValues) {
                            setDimensions($image);
                            setConstraints();
                        }
                    });

                    var throttledResizing = _.throttle(function () {
                        resizeImageToScale(scope.dimensions.scale.current);
                        calculateCropBox();
                    }, 15);

                    // Happens when we change the scale
                    scope.$watch("dimensions.scale.current", function (newValue, oldValue) {
                        if (scope.loaded) {
                            throttledResizing();
                        }
                    });

                    // Init
                    $image.on("load", function () {
                        $timeout(function () {
                            init($image);
                        });
                    });
                }
            };
        });
