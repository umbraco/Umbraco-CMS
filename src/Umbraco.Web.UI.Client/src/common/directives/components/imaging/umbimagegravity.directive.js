(function() {
    'use strict';

    function umbImageGravityController($scope, $element, $timeout) {

        var vm = this;

        //Internal values for keeping track of the dot and the size of the editor
        vm.dimensions = {
            width: 0,
            height: 0,
            left: 0,
            top: 0
        };

        var htmlImage = null; //DOM element reference
        var htmlOverlay = null; //DOM element reference
        var draggable = null;

        vm.loaded = false;
        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$postLink = postLink;
        vm.$onDestroy = onDestroy;
        vm.style = style;
        vm.setFocalPoint = setFocalPoint;

        /** Sets the css style for the Dot */
        function style() {

            if (vm.dimensions.width <= 0) {
                //this initializes the dimensions since when the image element first loads
                //there will be zero dimensions
                setDimensions();
            }

            return {
                'top': vm.dimensions.top + 'px',
                'left': vm.dimensions.left + 'px'
            };
        };

        function setFocalPoint (event) {
            $scope.$emit("imageFocalPointStart");

            var offsetX = event.offsetX - 10;
            var offsetY = event.offsetY - 10;

            calculateGravity(offsetX, offsetY);

            lazyEndEvent();
        };

        /** Initializes the component */
        function onInit() {
            if (!vm.center) {
                vm.center = { left: 0.5, top: 0.5 };
            }
        }

        /** Called when the component has linked everything and the DOM is available */
        function postLink() {
            //elements
            htmlImage = $element.find("img");
            htmlOverlay = $element.find(".overlay");

            //Drag and drop positioning, using jquery ui draggable
            draggable = htmlOverlay.draggable({
                containment: "parent",
                start: function () {
                    $scope.$apply(function () {
                        $scope.$emit("imageFocalPointStart");
                    });
                },
                stop: function () {
                    $scope.$apply(function () {
                        var offsetX = htmlOverlay[0].offsetLeft;
                        var offsetY = htmlOverlay[0].offsetTop;
                        calculateGravity(offsetX, offsetY);
                    });

                    lazyEndEvent();
                }
            });

            $(window).on('resize.umbImageGravity', function () {
                $scope.$apply(function () {
                    resized();
                });
            });

            //if any ancestor directive emits this event, we need to resize
            $scope.$on("editors.content.splitViewChanged", function () {
                $timeout(resized, 200);
            });

            //listen for the image DOM element loading
            htmlImage.on("load", function () {
                $timeout(function () {

                    vm.isCroppable = true;
                    vm.hasDimensions = true;
                    
                    if (vm.src) {
                        if (vm.src.endsWith(".svg")) {
                            vm.isCroppable = false;
                            vm.hasDimensions = false;
                        }
                        else {
                            // From: https://stackoverflow.com/a/51789597/5018
                            var type = vm.src.substring(vm.src.indexOf("/") + 1, vm.src.indexOf(";base64"));
                            if (type.startsWith("svg")) {
                                vm.isCroppable = false;
                                vm.hasDimensions = false;
                            }
                        }
                    }

                    setDimensions();
                    vm.loaded = true;
                    if (vm.onImageLoaded) {
                        vm.onImageLoaded({
                            "isCroppable": vm.isCroppable,
                            "hasDimensions": vm.hasDimensions
                        });
                    }
                }, 100);
            });
        }

        function onDestroy() {
            $(window).off('resize.umbImageGravity');
            if (htmlOverlay) {
                //TODO: This should be destroyed but this will throw an exception:
                // "cannot call methods on draggable prior to initialization; attempted to call method 'destroy'"
                // I've tried lots of things and cannot get this to work, we weren't destroying before so hopefully
                // there's no mem leaks?
                //htmlOverlay.draggable("destroy");
            }
            if (htmlImage) {
                htmlImage.off("load");
            }
        }

        /** Called when we need to resize based on window or DOM dimensions to re-center the focal point */
        function resized() {
            $timeout(function () {
                setDimensions();
            });
            // Make sure we can find the offset values for the overlay(dot) before calculating
            // fixes issue with resize event when printing the page (ex. hitting ctrl+p inside the rte)
            if (htmlOverlay.is(':visible')) {
                var offsetX = htmlOverlay[0].offsetLeft;
                var offsetY = htmlOverlay[0].offsetTop;
                calculateGravity(offsetX, offsetY);
            }
        }

        /** Watches the one way binding changes */
        function onChanges(changes) {
            if (changes.center && !changes.center.isFirstChange()
                && changes.center.currentValue
                && !angular.equals(changes.center.currentValue, changes.center.previousValue)) {
                //when center changes update the dimensions
                setDimensions();
            }
        }

        /** Sets the width/height/left/top dimentions based on the image size and the "center" value */
        function setDimensions() {

            if (vm.isCroppable && htmlImage && vm.center) {
                vm.dimensions.width = htmlImage.width();
                vm.dimensions.height = htmlImage.height();
                vm.dimensions.left = vm.center.left * vm.dimensions.width - 10;
                vm.dimensions.top = vm.center.top * vm.dimensions.height - 10;
            }

            return vm.dimensions.width;
        };

        /**
         * based on the offset selected calculates the "center" value and calls the callback
         * @param {any} offsetX
         * @param {any} offsetY
         */
        function calculateGravity(offsetX, offsetY) {

            vm.onValueChanged({
                left: (offsetX + 10) / vm.dimensions.width,
                top: (offsetY + 10) / vm.dimensions.height
            });

            //vm.center.left = (offsetX + 10) / scope.dimensions.width;
            //vm.center.top = (offsetY + 10) / scope.dimensions.height;
        };

        var lazyEndEvent = _.debounce(function () {
            $scope.$apply(function () {
                $scope.$emit("imageFocalPointStop");
            });
        }, 2000);

    }

    var umbImageGravityComponent = {
        templateUrl: 'views/components/imaging/umb-image-gravity.html',
        bindings: {
            src: "<",
            center: "<", 
            onImageLoaded: "&?",
            onValueChanged: "&"
        },
        controllerAs: 'vm',
        controller: umbImageGravityController
    };

    angular.module("umbraco.directives")
        .component('umbImageGravity', umbImageGravityComponent);

})();
