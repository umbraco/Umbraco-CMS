(function () {
    'use strict';

    function TourDirective($timeout, $http, tourService) {

        function link(scope, el, attr, ctrl) {

            scope.totalSteps;
            scope.currentStepIndex;
            scope.currentStep;
            scope.loadingStep = false;
            var popover;

            scope.nextStep = function() {
                nextStep();
            };

            scope.endTour = function() {
                tourService.endTour();
            };

            scope.completeTour = function() {
                tourService.completeTour();
            };

            function onInit() {
                popover = el.find(".umb-tour__popover");
                scope.totalSteps = scope.steps.length;
                scope.currentStepIndex = 0;
                startStep();
            }

            function setView() {
                if (scope.currentStep.view && scope.options.alias) {
                    //we do this to avoid a hidden dialog to start loading unconfigured views before the first activation
                    var configuredView = scope.currentStep.view;
                    if (scope.currentStep.view.indexOf(".html") === -1) {
                        var viewAlias = scope.currentStep.view.toLowerCase();
                        var tourAlias = scope.options.alias.toLowerCase();
                        configuredView = "views/common/tours/" + tourAlias + "/" + viewAlias + "/" + viewAlias + ".html";
                    }
                    if (configuredView !== scope.configuredView) {
                        scope.configuredView = configuredView;
                    }
                }
            }

            function nextStep() {
                scope.currentStepIndex++;
                if(scope.currentStepIndex !== scope.steps.length) {
                    startStep();
                }
            }

            function startStep() {

                // we need to make sure that all requests are done
                var timer = window.setInterval(function(){

                    console.log("pending", $http.pendingRequests.length);
                    console.log("document ready", document.readyState);
                    
                    scope.loadingStep = true;

                    // check for pending requests both in angular and on the document
                    if($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        console.log("Everything is DONE JOHN");

                        scope.currentStep = scope.steps[scope.currentStepIndex];

                        clearInterval(timer);

                        setView();
                        
                        positionPopover();

                        // if a custom event needs to be bound we do it now
                        if(scope.currentStep.event) {
                            bindEvent();
                        }

                        scope.loadingStep = false;

                    }
                    
                }, 100);

            }

            function positionPopover() {

                $timeout(function(){
                    
                    var element = $(scope.currentStep.element);                    
                    var scrollParent = element.scrollParent();

                    console.log("scrollParent", scrollParent);

                    // Detect if scroll is needed
                    if(element[0].offsetTop > scrollParent[0].clientHeight) {
                        console.log("SCROOOOOOOL");
                        scrollParent.animate({
                            scrollTop: element[0].offsetTop
                        }, function() {
                            // Animation complete.
                            console.log("ANIMATION COMPLETE");
                            _position();
                        });
                    } else {
                        _position();
                    }

                });

                function _position() {

                    var element = $(scope.currentStep.element);
                    var offset = element.offset();
                    var width = element.outerWidth(true);
                    var height = element.outerHeight(true);

                    $timeout(function(){
                    
                        var popoverBox = $(".umb-tour__popover");
                        var popoverWidth = popoverBox.outerWidth();
                        var popoverHeight = popoverBox.outerHeight();

                        console.log("This element", element);
                        console.log("width", width);
                        console.log("height", height);
                        console.log(scope.currentStep.placement);
                        console.log("popoverWidth", popoverWidth);
                        console.log("popoverHeight", popoverHeight);
                        console.log("element offset", offset);

                        // Element placements
                        if (scope.currentStep.placement === "top") {
                            popover.css({top: offset.top - popoverHeight, left: offset.left});
                        } else if (scope.currentStep.placement === "bottom") {
                            popover.css({top: offset.top + height, left: offset.left});
                        } else if (scope.currentStep.placement === "right") {
                            popover.css({top: offset.top, left: offset.left + width});
                        } else if (scope.currentStep.placement === "left") {
                            popover.css({top: offset.top, left: offset.left - popoverWidth});
                        } else if (scope.currentStep.placement === "center") {
                            popover.css({top: "50%", left: "50%", transform: "translate(-50%, -50%)"});
                        } else {
                            popover.css({top: "50%", left: "50%", transform: "translate(-50%, -50%)"});
                        }                        
                        
                    });

                    // SVG + jQuery backdrop

                    var target = $(scope.currentStep.element);
                    
                    // Rounding numbers
                    var topDistance = offset.top.toFixed();
                    var topAndHeight = (offset.top + height).toFixed();
                    var leftDistance = offset.left.toFixed();
                    var leftAndWidth = (offset.left + width).toFixed();
                    
                    // Convert classes into variables
                    var rectLeft = $(".rect-left")
                    var rectTop = $(".rect-top")
                    var rectBot = $(".rect-bot")
                    var rectRight = $(".rect-right")

                    // SVG rect at the left side of the canvas
                    rectLeft.css("width", leftDistance);
                    
                    // SVG rect at the top of the canvas
                    rectTop.css("height", topDistance);
                    rectTop.css("x", leftDistance);
                    
                    // SVG rect at the bottom of the canvas
                    rectBot.css("height", "100%");
                    rectBot.css("y", topAndHeight );
                    rectBot.css("x", leftDistance);
                    
                    // SVG rect at the right side of the canvas
                    rectRight.css("x", leftAndWidth);
                    rectRight.css("y", topDistance);
                    rectRight.css("height", height);

                }

            }

            function bindEvent() {
                if(scope.currentStep.clickElement) {
                    $(scope.currentStep.clickElement).on(scope.currentStep.event, handleEvent);
                } else {
                    $(scope.currentStep.element).on(scope.currentStep.event, handleEvent);
                }
            }

            function unbindEvent() {
                if(scope.currentStep.clickElement) {
                    $(scope.currentStep.clickElement).off(scope.currentStep.event);
                } else {
                    $(scope.currentStep.element).off(scope.currentStep.event);
                }
            }

            function handleEvent() {
                alert("event happened");
                unbindEvent();
                nextStep();
            }

            function resize() {
                positionPopover();
            }

            onInit();

            $(window).on('resize.umbTour', resize);

            scope.$on('$destroy', function () {
                $(window).off('.resize.umbTour');
            });

        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umb-tour.html',
            link: link,
            scope: {
                options: "=",
                steps: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTour', TourDirective);

})();
