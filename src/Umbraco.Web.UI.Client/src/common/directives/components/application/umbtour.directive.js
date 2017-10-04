(function () {
    'use strict';

    function TourDirective($timeout, $http, tourService) {

        function link(scope, el, attr, ctrl) {

            scope.totalSteps;
            scope.currentStepIndex;
            scope.currentStep;
            scope.loadingStep = false;
            var dot;

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
                dot = el.find(".umb-tour__dot");
                scope.totalSteps = scope.steps.length;
                scope.currentStepIndex = 0;
                
                startStep();
            }

            function nextStep() {
                scope.currentStepIndex++;
                if(scope.currentStepIndex !== scope.steps.length) {
                    startStep();
                }
            }

            function startStep() {

                scope.currentStep = scope.steps[scope.currentStepIndex];

                var timer = window.setInterval(function(){

                    console.log("pending", $http.pendingRequests.length);
                    console.log("document ready", document.readyState);
                    
                    scope.loadingStep = true;

                    if($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        console.log("Everything is DONE JOHN");
                        clearInterval(timer);
                        
                        positionDot();

                        if(scope.currentStep.event) {
                            bindEvent();
                        }

                        scope.loadingStep = false;

                    }
                    
                }, 100);

            }

            function positionDot() {

                $timeout(function(){
                    
                    var element = $(scope.currentStep.element);
                    var offset = element.offset();
                    var width = element.outerWidth(true);
                    var height = element.outerHeight(true);
    
                    console.log("This element", element);                    
                    console.log("width", width);
                    console.log("height", height);
          
                    dot.css({top: offset.top + (height / 2), left: offset.left + width});

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

                });

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
                positionDot();
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
