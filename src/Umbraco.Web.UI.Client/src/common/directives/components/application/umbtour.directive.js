(function () {
    'use strict';

    function TourDirective($timeout, $http, tourService, backdropService) {

        function link(scope, el, attr, ctrl) {

            var popover;

            scope.totalSteps;
            scope.currentStepIndex;
            scope.currentStep;
            scope.loadingStep = false;

            scope.nextStep = function() {
                nextStep();
            };

            scope.endTour = function() {
                unbindEvent();
                tourService.endTour();
                backdropService.close();
            };

            scope.completeTour = function() {
                unbindEvent();
                tourService.completeTour(scope.tour);
                backdropService.close();
            };

            function onInit() {
                popover = el.find(".umb-tour__popover");
                scope.totalSteps = scope.tour.steps.length;
                scope.currentStepIndex = 0;
                backdropService.open({disableEventsOnClick: true});
                startStep();
            }

            function setView() {
                if (scope.currentStep.view && scope.tour.alias) {
                    //we do this to avoid a hidden dialog to start loading unconfigured views before the first activation
                    var configuredView = scope.currentStep.view;
                    if (scope.currentStep.view.indexOf(".html") === -1) {
                        var viewAlias = scope.currentStep.view.toLowerCase();
                        var tourAlias = scope.tour.alias.toLowerCase();
                        configuredView = "views/common/tours/" + tourAlias + "/" + viewAlias + "/" + viewAlias + ".html";
                    }
                    if (configuredView !== scope.configuredView) {
                        scope.configuredView = configuredView;
                    }
                } else {
                    scope.configuredView = null;
                }
            }

            function nextStep() {
                scope.currentStepIndex++;
                if(scope.currentStepIndex !== scope.tour.steps.length) {
                    startStep();
                }
            }

            function startStep() {

                // we need to make sure that all requests are done
                var timer = window.setInterval(function(){

                    console.log("pending", $http.pendingRequests.length);
                    console.log("document ready", document.readyState);
                    
                    scope.loadingStep = true;

                    backdropService.setHighlight(null);

                    // check for pending requests both in angular and on the document
                    if($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        console.log("Everything is DONE JOHN");

                        scope.currentStep = scope.tour.steps[scope.currentStepIndex];

                        clearInterval(timer);

                        setView();
                        
                        positionPopover();

                        // if a custom event needs to be bound we do it now
                        if(scope.currentStep.event) {
                            bindEvent();
                        }

                        scope.loadingStep = false;

                    }
                    
                }, 50);

            }

            function positionPopover() {

                $timeout(function () {

                    var element = $(scope.currentStep.element);
                    var scrollParent = element.scrollParent();

                    console.log("scrollParent", scrollParent);

                    // Detect if scroll is needed
                    if (element[0].offsetTop > scrollParent[0].clientHeight) {
                        console.log("SCROOOOOOOL");
                        scrollParent.animate({
                            scrollTop: element[0].offsetTop
                        }, function () {
                            // Animation complete.
                            console.log("ANIMATION COMPLETE");
                            _position();
                            backdropService.setHighlight(scope.currentStep.element);
                        });
                    } else {
                        _position();
                        backdropService.setHighlight(scope.currentStep.element);
                    }

                });

                function _position() {

                    var element = $(scope.currentStep.element);
                    var offset = element.offset();
                    var width = element.outerWidth(true);
                    var height = element.outerHeight(true);

                    $timeout(function () {

                        var popoverWidth = popover.outerWidth(true);
                        var popoverHeight = popover.outerHeight(true);
                        var popoverOffset = popover.offset();
                        var documentWidth = $(document).width();
                        var documentHeight = $(document).height();
                        var position;
                        var css = {};

                        console.log("SPACE", space);
                        console.log("document width", documentWidth);
                        console.log("document height", documentHeight);
                        


                        // If no specific position is set - find the position with most available space
                        if(scope.currentStep.placement) {
                            
                            position = scope.currentStep.placement;

                        } else {

                            var space = {
                                "top": offset.top,
                                "right": documentWidth - (offset.left + width),
                                "bottom": documentHeight - (offset.top + height),
                                "left": offset.left
                            };

                            position = findMax(space);
                        }

                        if(position === "top") {

                            if (offset.left < documentWidth/2) {
                                css = {top: offset.top - popoverHeight, left: offset.left};        
                         
                            } else {
                                css = {top: offset.top - popoverHeight, left: offset.left - popoverWidth + width};
                            }
                        }

                        if(position === "right") {

                            if (offset.top < documentHeight/2) {
                                css = {top: offset.top, left: offset.left + width};        
                         
                            } else {
                                css = {top: offset.top + height - popoverHeight, left: offset.left + width};
                            }
                        }

                        if(position === "bottom") {

                            if (offset.left < documentWidth/2) {
                                css = {top: offset.top + height, left: offset.left};        
                         
                            } else {
                                css = {top: offset.top + height, left: offset.left - popoverWidth + width};
                            }
                        }

                        if(position === "left") {

                            if (offset.top < documentHeight/2) {
                                css = {top: offset.top, left: offset.left - popoverWidth};        
                         
                            } else {
                                css = {top: offset.top + height - popoverHeight, left: offset.left - popoverWidth};
                            }
                        }

                        popover.css(css);

                        scope.position = position;

                    });

                }

            }

            function findMax(obj) {
                var keys = Object.keys(obj);
                var max = keys[0];
                for (var i = 1, n = keys.length; i < n; ++i) {
                    var k = keys[i];
                    if (obj[k] > obj[max]) {
                        max = k;
                    }
                }
                return max;
            }

            function bindEvent() {
                var eventName = scope.currentStep.event + ".step-" + scope.currentStepIndex;
                if(scope.currentStep.clickElement) {
                    $(scope.currentStep.clickElement).on(eventName, handleEvent);
                    console.log("bind", eventName);                    
                } else {
                    $(scope.currentStep.element).on(eventName, handleEvent);
                    console.log("bind", eventName);                    
                }
            }

            function unbindEvent() {
                var eventName = scope.currentStep.event + ".step-" + scope.currentStepIndex;                
                if(scope.currentStep.clickElement) {
                    $(scope.currentStep.clickElement).off(eventName);
                    console.log("unbind", eventName);                    
                } else {
                    $(scope.currentStep.element).off(eventName);
                    console.log("unbind", eventName);                    
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
                $(window).off('resize.umbTour');
            });

        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umb-tour.html',
            link: link,
            scope: {
                tour: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTour', TourDirective);

})();
