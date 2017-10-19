(function () {
    'use strict';

    function TourDirective($timeout, $http, $q, tourService, backdropService) {

        function link(scope, el, attr, ctrl) {

            var popover;

            scope.loadingStep = false;
            scope.elementNotFound = false;

            scope.model.nextStep = function() {
                nextStep();
            };

            scope.model.endTour = function() {
                unbindEvent();
                tourService.endTour();
                backdropService.close();
            };

            scope.model.completeTour = function() {
                unbindEvent();
                tourService.completeTour(scope.model);
                backdropService.close();
            };

            function onInit() {
                popover = el.find(".umb-tour__popover");
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
                        // remove backdrop hightlight
                        backdropService.setHighlight(null);
                    });
                }
            }

            function startStep() {

                scope.loadingStep = true;
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
                    if(!scope.model.currentStep.element) {
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

                    console.log("scrollParent", scrollParent);

                    // Detect if scroll is needed
                    if (element[0].offsetTop > scrollParent[0].clientHeight) {
                        console.log("SCROOOOOOOL");
                        scrollParent.animate({
                            scrollTop: element[0].offsetTop
                        }, function () {
                            // Animation complete.
                            console.log("ANIMATION COMPLETE");
                            setPopoverPosition(element);
                            backdropService.setHighlight(scope.model.currentStep.element);
                        });
                    } else {
                        setPopoverPosition(element);
                        backdropService.setHighlight(scope.model.currentStep.element);
                    }

                });

            }

            function setPopoverPosition(element) {

                $timeout(function () {

                    var position = "center";
                    var css = {top: "50%", left: "50%", marginLeft: "inherit", marginTop: "inherit" };

                    var popoverWidth = popover.outerWidth();
                    var popoverHeight = popover.outerHeight();
                    var popoverOffset = popover.offset();
                    var documentWidth = $(document).width();
                    var documentHeight = $(document).height();

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

                        console.log("SPACE", space);
                        console.log("document width", documentWidth);
                        console.log("document height", documentHeight);

                        // get the posistion with most available space
                        position = findMax(space);

                        if (position === "top") {
                            if (offset.left < documentWidth / 2) {
                                css.top = offset.top - popoverHeight;
                                css.left = offset.left;
                            } else {
                                css.top = offset.top - popoverHeight;
                                css.left = offset.left - popoverWidth + width;
                            }
                        }

                        if (position === "right") {
                            if (offset.top < documentHeight / 2) {
                                css.top = offset.top; 
                                css.left = offset.left + width;
                            } else {
                                css.top = offset.top + height - popoverHeight;
                                css.left = offset.left + width;
                            }
                        }

                        if (position === "bottom") {
                            if (offset.left < documentWidth / 2) {
                                css.top = offset.top + height;
                                css.left = offset.left;
                            } else {
                                css.top = offset.top + height;
                                css.left = offset.left - popoverWidth + width;
                            }
                        }

                        if (position === "left") {
                            if (offset.top < documentHeight / 2) {
                                css.top = offset.top; 
                                css.left = offset.left - popoverWidth;
                            } else {
                                css.top = offset.top + height - popoverHeight;
                                css.left = offset.left - popoverWidth;
                            }
                        }

                    } else {

                        // if there is no dom element center the popover
                        css.marginLeft = - (popoverWidth / 2);
                        css.marginTop = - (popoverHeight / 2);

                    }

                    popover.css(css);

                    scope.position = position;

                });

            }

            function waitForPendingRerequests() {
                var deferred = $q.defer();
                var timer = window.setInterval(function(){
                    // check for pending requests both in angular and on the document
                    if($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        deferred.resolve();
                        clearInterval(timer);
                        scope.$apply();
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
                var eventName = scope.model.currentStep.event + ".step-" + scope.model.currentStepIndex;
                if(scope.model.currentStep.eventElement) {
                    $(scope.model.currentStep.eventElement).on(eventName, handleEvent);
                    console.log("bind", eventName);
                } else {
                    $(scope.model.currentStep.element).on(eventName, handleEvent);
                    console.log("bind", eventName);
                }
            }

            function unbindEvent() {
                var eventName = scope.model.currentStep.event + ".step-" + scope.model.currentStepIndex;                
                if(scope.model.currentStep.eventElement) {
                    $(scope.model.currentStep.eventElement).off(eventName);
                    console.log("unbind", eventName);
                } else {
                    $(scope.model.currentStep.element).off(eventName);
                    console.log("unbind", eventName);
                }
            }

            function handleEvent() {
                //alert("event happened");
                unbindEvent();
                nextStep();
            }

            function resize() {
                findHighlightElement();
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
                model: "="
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbTour', TourDirective);

})();
