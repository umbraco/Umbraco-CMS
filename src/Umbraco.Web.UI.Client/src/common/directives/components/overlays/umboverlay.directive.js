/**
@name umbraco.directives.directive:umbOverlay*
@deprecated
@restrict E
@scope

@description

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.Controller as vm">

        <button type="button" ng-click="vm.openOverlay()"></button>

        <umb-overlay
            ng-if="vm.overlay.show"
            model="vm.overlay"
            view="vm.overlay.view"
            position="right">
        </umb-overlay>

    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {

        "use strict";

        function Controller() {

            var vm = this;

            vm.openOverlay = openOverlay;

            function openOverlay() {

                vm.overlay = {
                    view: "mediapicker",
                    show: true,
                    submit: function(model) {

                        vm.overlay.show = false;
                        vm.overlay = null;
                    },
                    close: function(oldModel) {
                        vm.overlay.show = false;
                        vm.overlay = null;
                    }
                }

            };

        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

<h1>General Options</h1>
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.title</td>
        <td>String</td>
        <td>Set the title of the overlay.</td>
    </tr>
    <tr>
        <td>model.subtitle</td>
        <td>String</td>
        <td>Set the subtitle of the overlay.</td>
    </tr>
    <tr>
        <td>model.submitButtonLabel</td>
        <td>String</td>
        <td>Set an alternate submit button text</td>
    </tr>
    <tr>
        <td>model.submitButtonLabelKey</td>
        <td>String</td>
        <td>Set an alternate submit button label key for localized texts</td>
    </tr>
    <tr>
        <td>model.submitButtonState</td>
        <td>String</td>
        <td>Set the state for the submit button</td>
    </tr>
    <tr>
        <td>model.hideSubmitButton</td>
        <td>Boolean</td>
        <td>Hides the submit button</td>
    </tr>
    <tr>
        <td>model.closeButtonLabel</td>
        <td>String</td>
        <td>Set an alternate close button text</td>
    </tr>
    <tr>
        <td>model.closeButtonLabelKey</td>
        <td>String</td>
        <td>Set an alternate close button label key for localized texts</td>
    </tr>
    <tr>
        <td>model.show</td>
        <td>Boolean</td>
        <td>Show/hide the overlay</td>
    </tr>
    <tr>
        <td>model.submit</td>
        <td>Function</td>
        <td>Callback function when the overlay submits. Returns the overlay model object</td>
    </tr>
    <tr>
        <td>model.close</td>
        <td>Function</td>
        <td>Callback function when the overlay closes. Returns a copy of the overlay model object before being modified</td>
    </tr>
</table>

<h1>Item Picker</h1>
Opens an item picker.</br>
<strong>view: </strong>itempicker
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.availableItems</td>
            <td>Array</td>
            <td>Array of available items</td>
        </tr>
        <tr>
            <td>model.selectedItems</td>
            <td>Array</td>
            <td>Array of selected items. When passed in the selected items will be filtered from the available items.</td>
        </tr>
        <tr>
            <td>model.filter</td>
            <td>Boolean</td>
            <td>Set to false to hide the filter</td>
        </tr>
    </tbody>
</table>
<table>
    <thead>
        <tr>
            <th>Returns</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tr>
        <td>model.selectedItem</td>
        <td>Object</td>
        <td>The selected item</td>
    </tr>
</table>

<h1>YSOD</h1>
Opens an overlay to show a custom YSOD. </br>
<strong>view: </strong>ysod
<table>
    <thead>
        <tr>
            <th>Param</th>
            <th>Type</th>
            <th>Details</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>model.error</td>
            <td>Object</td>
            <td>Error object</td>
        </tr>
    </tbody>
</table>

@param {object} model Overlay options.
@param {string} view Path to view or one of the default view names.
@param {string} position The overlay position ("left", "right", "center": "target").
**/

(function () {
    'use strict';

    function OverlayDirective($timeout, formHelper, overlayHelper, localizationService, $q, $templateCache, $http, $compile) {

        function link(scope, el, attr, ctrl) {

            scope.directive = {
                enableConfirmButton: false
            };

            var overlayNumber = 0;
            var numberOfOverlays = 0;
            var isRegistered = false;


            var modelCopy = {};
            var unsubscribe = [];

            function activate() {
                setView();

                setButtonText();

                modelCopy = makeModelCopy(scope.model);

                $timeout(function () {

                    if (!scope.name) {
                        scope.name = 'overlay';
                    }

                    if (scope.position === "target" && scope.model.event) {
                        setTargetPosition();

                        // update the position of the overlay on content changes
                        // as these affect the layout/size of the overlay
                        if ('ResizeObserver' in window) {
                            var resizeObserver = new ResizeObserver(setTargetPosition);
                            var contentArea = document.getElementById("contentwrapper");
                            resizeObserver.observe(el[0]);
                            if (contentArea) {
                                resizeObserver.observe(contentArea);
                            }
                            unsubscribe.push(function () {
                                resizeObserver.disconnect();
                            });
                        }
                    }

                    // this has to be done inside a timeout to ensure the destroy
                    // event on other overlays is run before registering a new one
                    registerOverlay();

                    setOverlayIndent();

                    focusOnOverlayHeading()
                });

            }

            // Ideally this would focus on the first natively focusable element in the overlay, but as the content can be dynamic, it is focusing on the heading.
            function focusOnOverlayHeading() {
                var heading = el.find(".umb-overlay__title");

                if (heading) {
                    heading.focus();
                }
            }

            function setView() {

                if (scope.view) {

                    if (scope.view.indexOf(".html") === -1) {
                        var viewAlias = scope.view.toLowerCase();
                        scope.view = "views/common/overlays/" + viewAlias + "/" + viewAlias + ".html";
                    }

                    //if a custom parent scope is defined then we need to manually compile the view
                    if (scope.parentScope) {
                        var element = el.find(".scoped-view");
                        $http.get(scope.view, { cache: $templateCache })
                            .then(function (response) {
                                var templateScope = scope.parentScope.$new();
                                unsubscribe.push(function () {
                                    templateScope.$destroy();
                                });
                                templateScope.model = scope.model;
                                element.html(response.data);
                                element.show();
                                $compile(element)(templateScope);
                            });
                    }
                }

            }

            function setButtonText() {

                var labelKeys = [
                    "general_close",
                    "general_submit"
                ];

                localizationService.localizeMany(labelKeys).then(function (values) {
                    if (!scope.model.closeButtonLabelKey && !scope.model.closeButtonLabel) {
                        scope.model.closeButtonLabel = values[0];
                    }
                    if (!scope.model.submitButtonLabelKey && !scope.model.submitButtonLabel) {
                        scope.model.submitButtonLabel = values[1];
                    }
                });
            }

            function registerOverlay() {

                overlayNumber = overlayHelper.registerOverlay();

                $(document).on("keydown.overlay-" + overlayNumber, function (event) {

                    if (event.which === 27) {

                        numberOfOverlays = overlayHelper.getNumberOfOverlays();

                        if (numberOfOverlays === overlayNumber && !scope.model.disableEscKey) {
                            scope.$apply(function () {
                                scope.closeOverLay();
                            });
                        }

                        event.stopPropagation();
                        event.preventDefault();
                    }

                    if (event.which === 13) {

                        numberOfOverlays = overlayHelper.getNumberOfOverlays();

                        if (numberOfOverlays === overlayNumber) {

                            var activeElementType = document.activeElement.tagName;
                            var clickableElements = ["A", "BUTTON"];
                            var submitOnEnter = document.activeElement.hasAttribute("overlay-submit-on-enter");
                            var submitOnEnterValue = submitOnEnter ? document.activeElement.getAttribute("overlay-submit-on-enter") : "";

                            if (clickableElements.indexOf(activeElementType) >= 0) {
                                // don't do anything, let the browser Enter key handle this
                            } else if (activeElementType === "TEXTAREA" && !submitOnEnter) {


                            } else if (submitOnEnter && submitOnEnterValue === "false") {
                                // don't do anything
                            } else {
                                scope.$apply(function () {
                                    scope.submitForm(scope.model);
                                });
                                event.preventDefault();
                            }

                        }

                    }

                });

                isRegistered = true;

            }

            function unregisterOverlay() {

                if (isRegistered) {

                    overlayHelper.unregisterOverlay();

                    $(document).off("keydown.overlay-" + overlayNumber);

                    isRegistered = false;
                }

            }

            function makeModelCopy(object) {

                var newObject = {};

                for (var key in object) {
                    if (key !== "event" && key !== "parentScope") {
                        newObject[key] = Utilities.copy(object[key]);
                    }
                }

                return newObject;

            }

            function setOverlayIndent() {

                var overlayIndex = overlayNumber - 1;
                var indentSize = overlayIndex * 20;
                var overlayWidth = el[0].clientWidth;

                el.css('width', overlayWidth - indentSize);

                if (scope.position === "center" && overlayIndex > 0 || scope.position === "target" && overlayIndex > 0) {
                    var overlayTopPosition = el[0].offsetTop;
                    el.css('top', overlayTopPosition + indentSize);
                }

            }

            function setTargetPosition() {

                var overlay = $(scope.model.event.target).closest('.umb-overlay');
                var container = overlay.length > 0 ? overlay : $("#contentwrapper");

                let rect = container[0].getBoundingClientRect();

                var containerLeft = rect.left;
                var containerRight = containerLeft + rect.width;
                var containerTop = rect.top;
                var containerBottom = containerTop + rect.height;

                var mousePositionClickX = null;
                var mousePositionClickY = null;
                var elementHeight = null;
                var elementWidth = null;

                var position = {
                    right: "inherit",
                    left: "inherit",
                    top: "inherit",
                    bottom: "inherit"
                };

                // click position
                mousePositionClickX = scope.model.event.pageX;
                mousePositionClickY = scope.model.event.pageY;

                // element size
                elementHeight = el[0].clientHeight;
                elementWidth = el[0].clientWidth;

                // move element to this position
                // when using hotkey it fallback to center of container
                position.left = mousePositionClickX ? mousePositionClickX - (elementWidth / 2) : (containerLeft + containerRight) / 2 - (elementWidth / 2);
                position.top = mousePositionClickY ? mousePositionClickY - (elementHeight / 2) : (containerTop + containerBottom) / 2 - (elementHeight / 2);

                // check to see if element is outside screen
                // outside right
                if (position.left + elementWidth > containerRight) {
                    position.right = 10;
                    position.left = "inherit";
                }

                // outside bottom
                if (position.top + elementHeight > containerBottom) {
                    position.bottom = 10;
                    position.top = "inherit";
                }

                // outside left
                if (position.left < containerLeft) {
                    position.left = containerLeft + 10;
                    position.right = "inherit";
                }

                // outside top
                if (position.top < containerTop) {
                    position.top = 10;
                    position.bottom = "inherit";
                }

                el.css(position);
                el.css("visibility", "visible");
            }

            scope.submitForm = function (model) {
                if (scope.model.submit) {
                    if (formHelper.submitForm({ scope: scope, skipValidation: scope.model.skipFormValidation, keepServerValidation: true })) {

                        if (scope.model.confirmSubmit && scope.model.confirmSubmit.enable && !scope.directive.enableConfirmButton) {
                            //wrap in a when since we don't know if this is a promise or not
                            $q.when(scope.model.submit(model, modelCopy, scope.directive.enableConfirmButton)).then(
                                function () {
                                    formHelper.resetForm({ scope: scope });
                                });
                        } else {
                            unregisterOverlay();
                            //wrap in a when since we don't know if this is a promise or not
                            $q.when(scope.model.submit(model, modelCopy, scope.directive.enableConfirmButton)).then(
                                function () {
                                    formHelper.resetForm({ scope: scope });
                                });
                        }

                    }
                }
            };

            scope.cancelConfirmSubmit = function () {
                scope.model.confirmSubmit.show = false;
            };

            scope.closeOverLay = function () {

                unregisterOverlay();

                if (scope.model && scope.model.close) {
                    scope.model = modelCopy;
                    scope.model.close(scope.model);
                } else {
                    scope.model.show = false;
                    scope.model = null;
                }

            };

            scope.outSideClick = function () {
                if (!scope.model.disableBackdropClick) {
                    scope.closeOverLay();
                }
            };

            unsubscribe.push(unregisterOverlay);
            scope.$on('$destroy', function () {
                for (var i = 0; i < unsubscribe.length; i++) {
                    unsubscribe[i]();
                }
            });

            activate();

        }

        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/overlays/umb-overlay.html',
            scope: {
                ngShow: "=",
                model: "=",
                view: "=",
                position: "@",
                size: "=?",
                name: "=?",
                parentScope: "=?"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbOverlay', OverlayDirective);

})();
