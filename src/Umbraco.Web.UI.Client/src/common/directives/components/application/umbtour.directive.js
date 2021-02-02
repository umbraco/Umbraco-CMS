/**
@ngdoc directive
@name umbraco.directives.directive:umbTour
@restrict E
@scope

@description
<b>Added in Umbraco 7.8</b>. The tour component is a global component and is already added to the umbraco markup.
In the Umbraco UI the tours live in the "Help drawer" which opens when you click the Help-icon in the bottom left corner of Umbraco.
You can easily add you own tours to the Help-drawer or show and start tours from
anywhere in the Umbraco backoffice. To see a real world example of a custom tour implementation, install <a href="https://our.umbraco.com/projects/starter-kits/the-starter-kit/">The Starter Kit</a> in Umbraco 7.8

<h1><b>Extending the help drawer with custom tours</b></h1>
The easiet way to add new tours to Umbraco is through the Help-drawer. All it requires is a my-tour.json file.
Place the file in <i>App_Plugins/{MyPackage}/backoffice/tours/{my-tour}.json</i> and it will automatically be
picked up by Umbraco and shown in the Help-drawer.

<h3><b>The tour object</b></h3>
The tour object consist of two parts - The overall tour configuration and a list of tour steps. We have split up the tour object for a better overview.
<pre>
// The tour config object
{
    "name": "My Custom Tour", // (required)
    "alias": "myCustomTour", // A unique tour alias (required)
    "group": "My Custom Group" // Used to group tours in the help drawer
    "groupOrder": 200 // Control the order of tour groups
    "allowDisable": // Adds a "Don't" show this tour again"-button to the intro step
    "culture" : // From v7.11+. Specifies the culture of the tour (eg. en-US), if set the tour will only be shown to users with this culture set on their profile. If omitted or left empty the tour will be visible to all users
    "requiredSections":["content", "media", "mySection"] // Sections that the tour will access while running, if the user does not have access to the required tour sections, the tour will not load.
    "steps": [] // tour steps - see next example
}
</pre>
<pre>
// A tour step object
{
    "title": "Title",
    "content": "<p>Step content</p>",
    "type": "intro" // makes the step an introduction step,
    "element": "[data-element='my-table-row']", // the highlighted element
    "event": "click" // forces the user to click the UI to go to next step
    "eventElement": "[data-element='my-table-row'] [data-element='my-tour-button']" // specify an element to click inside a highlighted element
    "elementPreventClick": false // prevents user interaction in the highlighted element
    "backdropOpacity": 0.4 // the backdrop opacity
    "view": "" // add a custom view
    "customProperties" : {} // add any custom properties needed for the custom view
    "skipStepIfVisible": ".dashboard div [data-element='my-tour-button']" // if we can find this DOM element on the page then we will skip this step
}
</pre>

<h1><b>Adding tours to other parts of the Umbraco backoffice</b></h1>
It is also possible to add a list of custom tours to other parts of the Umbraco backoffice,
as an example on a Dashboard in a Custom section. You can then use the {@link umbraco.services.tourService tourService} to start and stop tours but you don't have to register them as part of the tour service.

<h1><b>Using the tour service</b></h1>
<h3>Markup example - show custom tour</h3>
<pre>
    <div ng-controller="My.TourController as vm">

        <div>{{vm.tour.name}}</div>
        <button type="button" ng-click="vm.startTour()">Start tour</button>

        <!-- This button will be clicked in the tour -->
        <button data-element="my-tour-button" type="button">Click me</button>

    </div>
</pre>

<h3>Controller example - show custom tour</h3>
<pre>
    (function () {
        "use strict";

        function TourController(tourService) {

            var vm = this;

            vm.tour = {
                "name": "My Custom Tour",
                "alias": "myCustomTour",
                "steps": [
                    {
                        "title": "Welcome to My Custom Tour",
                        "content": "",
                        "type": "intro"
                    },
                    {
                        "element": "[data-element='my-tour-button']",
                        "title": "Click the button",
                        "content": "Click the button",
                        "event": "click",
                        "skipStepIfVisible": "[data-element='my-other-tour-button']"
                    }
                ]
            };

            vm.startTour = startTour;

            function startTour() {
                tourService.startTour(vm.tour);
            }

        }

        angular.module("umbraco").controller("My.TourController", TourController);

    })();
</pre>

<h1><b>Custom step views</b></h1>
In some cases you will need a custom view for one of your tour steps. 
This could be for validation or for running any other custom logic for that step. 
We have added a couple of helper components to make it easier to get the step scaffolding to look like a regular tour step. 
In the following example you see how to run some custom logic before a step goes to the next step.

<h3>Markup example - custom step view</h3>
<pre>
    <div ng-controller="My.TourStep as vm">

        <umb-tour-step on-close="model.endTour()">
                
            <umb-tour-step-header
                title="model.currentStep.title">
            </umb-tour-step-header>
            
            <umb-tour-step-content
                content="model.currentStep.content">

                <!-- Add any custom content here  -->

            </umb-tour-step-content>

            <umb-tour-step-footer class="flex justify-between items-center">

                <umb-tour-step-counter
                    current-step="model.currentStepIndex + 1"
                    total-steps="model.steps.length">
                </umb-tour-step-counter>

                <div>
                    <umb-button 
                        size="xs" 
                        button-style="action" 
                        type="button" 
                        action="vm.initNextStep()" 
                        label="Next">
                    </umb-button>
                </div>

            </umb-tour-step-footer>

        </umb-tour-step>

    </div>
</pre>

<h3>Controller example - custom step view</h3>
<pre>
    (function () {
        "use strict";

        function StepController() {

            var vm = this;
            
            vm.initNextStep = initNextStep;

            function initNextStep() {
                // run logic here before going to the next step
                $scope.model.nextStep();
            }

        }

        angular.module("umbraco").controller("My.TourStep", StepController);

    })();
</pre>


<h3>Related services</h3>
<ul>
    <li>{@link umbraco.services.tourService tourService}</li>
</ul>

@param {string} model (<code>binding</code>): Tour object

**/

(function () {
    'use strict';

    function TourDirective($timeout, $http, $q, tourService, backdropService) {

        function link(scope, el, attr, ctrl) {

            var popover;
            var pulseElement;
            var pulseTimer;

            scope.loadingStep = false;
            scope.elementNotFound = false;

            scope.model.nextStep = function () {
                nextStep();
            };

            scope.model.endTour = function () {
                unbindEvent();
                tourService.endTour(scope.model);
                backdropService.close();
            };

            scope.model.completeTour = function () {
                unbindEvent();
                tourService.completeTour(scope.model).then(function () {
                    backdropService.close();
                });
            };

            scope.model.disableTour = function () {
                unbindEvent();
                tourService.disableTour(scope.model).then(function () {
                    backdropService.close();
                });
            }

            function onInit() {
                popover = el.find(".umb-tour__popover");
                pulseElement = el.find(".umb-tour__pulse");
                popover.hide();
                scope.model.currentStepIndex = 0;
                backdropService.open({ disableEventsOnClick: true });
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
                if (scope.model.currentStepIndex !== scope.model.steps.length) {

                    var upcomingStep = scope.model.steps[scope.model.currentStepIndex];

                    // If the currentStep JSON object has 'skipStepIfVisible'
                    // It's a DOM selector - if we find it then we ship over this step
                    if (upcomingStep.skipStepIfVisible) {
                        let tryFindDomEl = document.querySelector(upcomingStep.skipStepIfVisible);
                        if (tryFindDomEl) {
                            // check if element is visible:
                            if( tryFindDomEl.offsetWidth || tryFindDomEl.offsetHeight || tryFindDomEl.getClientRects().length ) {
                                // if it was visible then we skip the step.
                                nextStep();
                                return;
                            }
                        }
                    }

                    startStep();
                } else {
                    // tour completed - final step
                    scope.loadingStep = true;

                    waitForPendingRerequests().then(function () {
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

                waitForPendingRerequests().then(function () {

                    scope.model.currentStep = scope.model.steps[scope.model.currentStepIndex];

                    setView();

                    // if highlight element is set - find it
                    findHighlightElement();

                    // if a custom event needs to be bound we do it now
                    if (scope.model.currentStep.event) {
                        bindEvent();
                    }

                    scope.loadingStep = false;

                });
            }

            function findHighlightElement() {

                scope.elementNotFound = false;

                $timeout(function () {
                    // clear element when step as marked as intro, so it always displays in the center
                    if (scope.model.currentStep && scope.model.currentStep.type === "intro") {
                        scope.model.currentStep.element = null;
                        scope.model.currentStep.eventElement = null;
                        scope.model.currentStep.event = null;
                    }

                    // if an element isn't set - show the popover in the center
                    if (scope.model.currentStep && !scope.model.currentStep.element) {
                        setPopoverPosition(null);
                        return;
                    }

                    var element = $(scope.model.currentStep.element);

                    // we couldn't find the element in the dom - abort and show error
                    if (element.length === 0) {
                        scope.elementNotFound = true;
                        setPopoverPosition(null);
                        return;
                    }

                    var scrollParent = element.scrollParent();
                    var el = element;
                    var offsetTop = 0;
                    if (scrollParent[0] === document) {
                        offsetTop = el[0].offsetTop;
                    } else {
                        while ($.contains(scrollParent[0], el[0])) {
                            offsetTop += el[0].offsetTop;
                            el = el.offsetParent();
                        }
                    }

                    var scrollToCenterOfContainer = offsetTop - (scrollParent[0].clientHeight / 2);
                    if (element[0].clientHeight < scrollParent[0].clientHeight) {
                        scrollToCenterOfContainer += (element[0].clientHeight / 2);
                    }

                    // Detect if scroll is needed
                    if (offsetTop > scrollParent[0].clientHeight - 200) {
                        scrollParent.animate({
                            scrollTop: scrollToCenterOfContainer
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
                    var documentWidth = $(document).width();
                    var documentHeight = $(document).height();

                    if (element) {

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
                            if (offset.top + popoverHeight < documentHeight) {
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
                            if (offset.top + popoverHeight < documentHeight) {
                                css.top = offset.top;
                                css.left = offset.left - popoverWidth - margin;
                            } else {
                                css.top = offset.top + height - popoverHeight;
                                css.left = offset.left - popoverWidth - margin;
                            }
                        }

                    } else {
                        // if there is no dom element center the popover
                        css.top = "calc(50% - " + popoverHeight / 2 + "px)";
                        css.left = "calc(50% - " + popoverWidth / 2 + "px)";
                    }

                    popover.css(css).fadeIn("fast");

                });


            }

            function setPulsePosition() {
                if (scope.model.currentStep.event) {

                    pulseTimer = $timeout(function () {

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
                var timer = window.setInterval(function () {

                    var requestsReady = false;
                    var animationsDone = false;

                    // check for pending requests both in angular and on the document
                    if ($http.pendingRequests.length === 0 && document.readyState === "complete") {
                        requestsReady = true;
                    }

                    // check for animations. ng-enter and ng-leave are default angular animations. 
                    // Also check for infinite editors animating
                    if (document.querySelectorAll(".ng-enter, .ng-leave, .umb-editor--animating").length === 0) {
                        animationsDone = true;
                    }

                    if (requestsReady && animationsDone) {
                        $timeout(function () {
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

                if (scope.model.currentStep.eventElement) {
                    bindToElement = scope.model.currentStep.eventElement;
                }

                $(bindToElement).on(eventName, function () {
                    if (!handled) {
                        unbindEvent();
                        nextStep();
                        handled = true;
                    }
                });

                // Hack: we do this to handle cases where ng-if is used and removes the element we need to click.
                // for some reason it seems the elements gets removed before the event is raised. This is a temp solution which assumes:
                // "if you ask me to click on an element, and it suddenly gets removed from the dom, let's go on to the next step".
                $(bindToElement).on(removeEventName, function () {
                    if (!handled) {
                        unbindEvent();
                        nextStep();
                        handled = true;
                    }
                });

            }

            function unbindEvent() {
                var eventName = scope.model.currentStep.event + ".step-" + scope.model.currentStepIndex;
                var removeEventName = "remove.step-" + scope.model.currentStepIndex;

                if (scope.model.currentStep.eventElement) {
                    $(scope.model.currentStep.eventElement).off(eventName);
                    $(scope.model.currentStep.eventElement).off(removeEventName);
                } else {
                    $(scope.model.currentStep.element).off(eventName);
                    $(scope.model.currentStep.element).off(removeEventName);
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
