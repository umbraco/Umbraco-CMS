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

        var imageElement = null; //DOM element reference
        var focalPointElement = null; //DOM element reference
        var draggable = null;

        vm.loaded = false;
        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$postLink = postLink;
        vm.$onDestroy = onDestroy;
        vm.style = {};
        vm.overlayStyle = {};
        vm.setFocalPoint = setFocalPoint;
        vm.resetFocalPoint = resetFocalPoint;

        /** Sets the css style for the Dot */
        function updateStyle() {
            vm.style = {
                'top': vm.dimensions.top + 'px',
                'left': vm.dimensions.left + 'px'
            };
            vm.overlayStyle = {
                'width': vm.dimensions.width + 'px',
                'height': vm.dimensions.height + 'px'
            };

        };

        function resetFocalPoint() {
            vm.onValueChanged({
                left: 0.5,
                top: 0.5
            });
        };

        function setFocalPoint(event) {
            $scope.$emit("imageFocalPointStart");

            // We do this to get the right position, no matter the focalPoint was clicked.
            var viewportPosition = imageElement[0].getBoundingClientRect();
            var offsetX = event.clientX - viewportPosition.left;
            var offsetY = event.clientY - viewportPosition.top;

            calculateGravity(offsetX, offsetY);
            $scope.$emit("imageFocalPointStop");
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
            imageElement = $element.find("img");
            focalPointElement = $element.find(".focalPoint");

            //Drag and drop positioning, using jquery ui draggable
            draggable = focalPointElement.draggable({
                containment: "parent",
                start: function () {
                    $scope.$emit("imageFocalPointStart");
                },
                stop: function (event, ui) {

                    var offsetX = ui.position.left;
                    var offsetY = ui.position.top;

                    $scope.$evalAsync(calculateGravity(offsetX, offsetY));

                    $scope.$emit("imageFocalPointStop");

                }
            });

            window.addEventListener('resize.umbImageGravity', onResizeHandler);
            window.addEventListener('resize', onResizeHandler);


            //if any ancestor directive emits this event, we need to resize
            $scope.$on("editors.content.splitViewChanged", function () {
                $timeout(resized, 200);
            });

            //listen for the image DOM element loading
            imageElement.on("load", function () {
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
                    updateStyle();

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
            window.removeEventListener('resize.umbImageGravity', onResizeHandler);
            window.removeEventListener('resize', onResizeHandler);
            /*
            if (focalPointElement) {
                // TODO: This should be destroyed but this will throw an exception:
                // "cannot call methods on draggable prior to initialization; attempted to call method 'destroy'"
                // I've tried lots of things and cannot get this to work, we weren't destroying before so hopefully
                // there's no mem leaks?
                focalPointElement.draggable("destroy");
            }
            */
            if (imageElement) {
                imageElement.off("load");
            }
        }

        /** Called when we need to resize based on window or DOM dimensions to re-center the focal point */
        function resized() {
            $timeout(function () {
                setDimensions();
                updateStyle();
            });
            /*
            // Make sure we can find the offset values for the overlay(dot) before calculating
            // fixes issue with resize event when printing the page (ex. hitting ctrl+p inside the rte)
            if (focalPointElement.is(':visible')) {
                var offsetX = focalPointElement[0].offsetLeft;
                var offsetY = focalPointElement[0].offsetTop;
                calculateGravity(offsetX, offsetY);
            }
            */
        }

        function onResizeHandler() {
            $scope.$evalAsync(resized);
        }

        /** Watches the one way binding changes */
        function onChanges(changes) {
            if (changes.center && !changes.center.isFirstChange()
                && changes.center.currentValue
                && !Utilities.equals(changes.center.currentValue, changes.center.previousValue)) {
                //when center changes update the dimensions
                setDimensions();
                updateStyle();
            }
        }

        /** Sets the width/height/left/top dimentions based on the image size and the "center" value */
        function setDimensions() {

            if (vm.isCroppable && imageElement && vm.center) {
                vm.dimensions.width = imageElement.width();
                vm.dimensions.height = imageElement.height();
                vm.dimensions.left = vm.center.left * vm.dimensions.width;
                vm.dimensions.top = vm.center.top * vm.dimensions.height;
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
                left: Math.min(Math.max(offsetX, 0), vm.dimensions.width) / vm.dimensions.width,
                top: Math.min(Math.max(offsetY, 0), vm.dimensions.height) / vm.dimensions.height
            });
        };

    }

    var umbImageGravityComponent = {
        templateUrl: 'views/components/imaging/umb-image-gravity.html',
        bindings: {
            src: "<",
            center: "<",
            onImageLoaded: "&?",
            onValueChanged: "&",
            disableFocalPoint: "<?"
        },
        controllerAs: 'vm',
        controller: umbImageGravityController
    };

    angular.module("umbraco.directives")
        .component('umbImageGravity', umbImageGravityComponent);

})();
