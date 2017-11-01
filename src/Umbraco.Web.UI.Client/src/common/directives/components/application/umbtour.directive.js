(function () {
    'use strict';

    function TourDirective($timeout, $http, $q, tourService, backdropService) {

        function link(scope, el, attr, ctrl) {

            var popover;
            var pulseElement;
            var pulseTimer;

            scope.loadingStep = false;
            scope.elementNotFound = false;

            scope.model.nextStep = function() {
                nextStep();
            };

            scope.model.endTour = function() {
                unbindEvent();
                tourService.endTour(scope.model);
                backdropService.close();
            };

            scope.model.completeTour = function() {
                unbindEvent();
                tourService.completeTour(scope.model);
                backdropService.close();
            };

            scope.model.disableTour = function() {
                unbindEvent();
                tourService.disableTour(scope.model);
                backdropService.close();
            }

            function onInit() {
                popover = el.find(".umb-tour__popover");
                pulseElement = el.find(".umb-tour__pulse");
                popover.hide();
                scope.model.currentStepIndex = 0;
                backdropService.open({disableEventsOnClick: true});
                startStep();
            }

            function setView() {
                if (scope.model.currentStep.view && scope.model.alias) {
                    //we do this to avoid a hidden dialog to start loading unconfigured views before the first activation
                    var configuredView = scope.model.currentStep.view;
                    if (scope.model.currentStep.view.indexOf(".html") === -1) {
                        var viewAlias = scope.model.currentStep.view.toLowerCase();
                        var tourAlias = scope.model.alias.toLowerCase();
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
                
                popover.hide();
                pulseElement.hide();
                $timeout.cancel(pulseTimer);
                scope.model.currentStepIndex++;

                // make sure we don't go too far
                if(scope.model.currentStepIndex !== scope.model.steps.length) {
                    startStep();
                // tour completed - final step
                } else {
                    scope.loadingStep = true;

                    waitForPendingRerequests().then(function(){
                        scope.loadingStep = false;
                        // clear current step
                        scope.model.currentStep = {};
                        // set popover position to center
                        setPopoverPosition(null);
                        // remove backdrop hightlight and custom opacity
                        backdropService.setHighlight(null);
                        backdropService.setOpacity(null);
                    });
                }
            }

            function startStep() {
                scope.loadingStep = true;
                backdropService.setOpacity(scope.model.steps[scope.model.currentStepIndex].backdropOpacity);
                backdropService.setHighlight(null);

                waitForPendingRerequests().then(function() {

                    scope.model.currentStep = scope.model.steps[scope.model.currentStepIndex];

                    setView();
                    
                    // if highlight element is set - find it
                    findHighlightElement();

                    // if a custom event needs to be bound we do it now
                    if(scope.model.currentStep.event) {
                        bindEvent();
                    }

                    scope.loadingStep = false;

                });
            }

            function findHighlightElement() {

                scope.elementNotFound = false;                

                $timeout(function () {

                    // if an element isn't set - show the popover in the center
                    if(scope.model.currentStep && !scope.model.currentStep.element) {
                        setPopoverPosition(null);
                        return;
                    }

                    var element = angular.element(scope.model.currentStep.element);

                    // we couldn't find the element in the dom - abort and show error
                    if(element.length === 0) {
                        scope.elementNotFound = true;
                        setPopoverPosition(null);
                        return;
                    }

                    var scrollParent = element.scrollParent();

                    // Detect if scroll is needed
                    if (element[0].offsetTop > scrollParent[0].clientHeight) {
                        scrollParent.animate({
                            scrollTop: element[0].offsetTop
                        }, function () {
                            // Animation complete.
                            setPopoverPosition(element);
                            setPulsePosition();
                            backdropService.setHighlight(scope.model.currentStep.element, scope.model.currentStep.elementPreventClick);
                        });
                    } else {
                        setPopoverPosition(element);
                        setPulsePosition();
                        backdropService.setHighlight(scope.model.currentStep.element, scope.model.currentStep.elementPreventClick);
                    }

                });

            }

            function setPopoverPosition(element) {

                $timeout(function () {
                    
                    var position = "center";
                    var margin = 20;
                    var css = {};

                    var popoverWidth = popover.outerWidth();
                    var popoverHeight = popover.outerHeight();
                    var popoverOffset = popover.offset();
                    var documentWidth = angular.element(document).width();
                    var documentHeight = angular.element(document).height();

                    if(element) {

                        var offset = element.offset();
                        var width = element.outerWidth();
                        var height = element.outerHeight();

                        // messure available space on each side of the target element
                        var space = {
                            "top": offset.top,
                            "right": documentWidth - (offset.left + width),
                            "bottom": documentHeight - (offset.top + height),
                            "left": offset.left
                        };

                        // get the posistion with most available space
                        position = findMax(space);

                        if (position === "top") {
                            if (offset.left < documentWidth / 2) {
                                css.top = offset.top - popoverHeight - margin;
                                css.left = offset.left;
                            } else {
                                css.top = offset.top - popoverHeight - margin;
                                css.left = offset.left - popoverWidth + width;
                            }
                        }

                        if (position === "right") {
                            if (offset.top < documentHeight / 2) {
                                css.top = offset.top;
                                css.left = offset.left + width + margin;
                            } else {
                                css.top = offset.top + height - popoverHeight;
                                css.left = offset.left + width + margin;
                            }
                        }

                        if (position === "bottom") {
                            if (offset.left < documentWidth / 2) {
                                css.top = offset.top + height + margin;
                                css.left = offset.left;
                            } else {
                                css.top = offset.top + height + margin;
                                css.left = offset.left - popoverWidth + width;
                            }
                        }

                        if (position === "left") {
                            if (offset.top < documentHeight / 2) {
                                css.top = offset.top;
                                css.left = offset.left - popoverWidth - margin;
                            } else {
                                css.top = offset.top + height - popoverHeight;
                                css.left = offset.left - popoverWidth - margin;
                            }
                        }

                    } else {
                        // if there is no dom element center the popover
                        css.top = "calc(50% - " + popoverHeight/2 + "px)";
                        css.left = "calc(50% - " + popoverWidth/2 + "px)";                        
                    }

                    popover.css(css).fadeIn("fast");
                    
                });


            }

            function setPulsePosition() {
                if(scope.model.currentStep.event) {

                    pulseTimer = $timeout(function(){
                        
                        var clickElementSelector = scope.model.currentStep.eventElement ? scope.model.currentStep.eventElement : scope.model.currentStep.element;
                        var clickElement = $(clickElementSelector);
        
                        var offset = clickElement.offset();
                        var width = clickElement.outerWidth();
                        var height = clickElement.outerHeight();
        
                        pulseElement.css({ "width": width, "height": height, "left": offset.left, "top": offset.top });
                        pulseElement.fadeIn();

                    }, 1000);
                }
            }

            function waitForPendingRerequests() {
                var deferred = $q.defer();
                var timer = window.setInterval(function(){
                    // check for pending requests both in angular and on the document
                    if($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        $timeout(function(){
                            deferred.resolve();
                            clearInterval(timer);
                        });
                    }
                }, 50);
                return deferred.promise;
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

                var bindToElement = scope.model.currentStep.element;
                var eventName = scope.model.currentStep.event + ".step-" + scope.model.currentStepIndex;
                var removeEventName = "remove.step-" + scope.model.currentStepIndex;
                var handled = false;  

                if(scope.model.currentStep.eventElement) {
                    bindToElement = scope.model.currentStep.eventElement;
                }

                $(bindToElement).on(eventName, function(){
                    if(!handled) {
                        unbindEvent();
                        nextStep();
                        handled = true;
                    }
                });

                // Hack: we do this to handle cases where ng-if is used and removes the element we need to click.
                // for some reason it seems the elements gets removed before the event is raised. This is a temp solution which assumes:
                // "if you ask me to click on an element, and it suddenly gets removed from the dom, let's go on to the next step".
                $(bindToElement).on(removeEventName, function () {
                    if(!handled) {
                        unbindEvent();
                        nextStep();
                        handled = true;
                    }
                });

            }

            function unbindEvent() {
                var eventName = scope.model.currentStep.event + ".step-" + scope.model.currentStepIndex;
                var removeEventName = "remove.step-" + scope.model.currentStepIndex;
                
                if(scope.model.currentStep.eventElement) {
                    angular.element(scope.model.currentStep.eventElement).off(eventName);
                    angular.element(scope.model.currentStep.eventElement).off(removeEventName);
                } else {
                    angular.element(scope.model.currentStep.element).off(eventName);
                    angular.element(scope.model.currentStep.element).off(removeEventName);
                }
            }

            function resize() {
                findHighlightElement();
            }

            onInit();

            $(window).on('resize.umbTour', resize);

            scope.$on('$destroy', function () {
                $(window).off('resize.umbTour');
                unbindEvent();
                $timeout.cancel(pulseTimer);
            });

        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/application/umb-tour.html',
            link: link,
            scope: {
                model: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTour', TourDirective);

})();
