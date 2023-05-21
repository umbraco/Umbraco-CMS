/**
* @ngdoc directive
* @name umbraco.directives.directive:navResize
* @restrict A
 *
 * @description
 * Handles how the navigation responds to window resizing and controls how the draggable resize panel works
**/
angular.module("umbraco.directives")
    .directive('navResize', function (appState, eventsService, windowResizeListener) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs, ctrl) {

                var minScreenSize = 1100;
                var resizeEnabled = false;

                function setTreeMode() {
                    appState.setGlobalState("showNavigation", appState.getGlobalState("isTablet") === false);
                }

                function enableResize() {
                    //only enable when the size is correct and it's not already enabled
                    if (!resizeEnabled && appState.getGlobalState("isTablet") === false) {
                        element.resizable(
                        {
                            containment: $("#mainwrapper"),
                            autoHide: true,
                            handles: "e",
                            alsoResize: ".navigation-inner-container",
                            resize: function(e, ui) {
                                var wrapper = $("#mainwrapper");
                                var contentPanel = $("#contentwrapper");
                                var umbNotification = $("#umb-notifications-wrapper");
                                var bottomBar = contentPanel.find(".umb-bottom-bar");
                                var navOffeset = $("#navOffset");

                                var leftPanelWidth = ui.element.width();

                                contentPanel.css({ left: leftPanelWidth });
                                bottomBar.css({ left: leftPanelWidth });
                                umbNotification.css({ left: leftPanelWidth });

                                navOffeset.css({ "margin-left": ui.element.outerWidth() });
                            },
                            stop: function (e, ui) {

                            }
                        });

                        resizeEnabled = true;
                    }
                }

                function resetResize() {
                    if (resizeEnabled) {
                        //kill the resize
                        element.resizable("destroy");
                        element.css("width", "");

                        var navInnerContainer = element.find(".navigation-inner-container");

                        navInnerContainer.css("width", "");
                        $("#contentwrapper").css("left", "");
                        $("#umb-notifications-wrapper").css("left", "");
                        $("#navOffset").css("margin-left", "");

                        resizeEnabled = false;
                    }
                }

                var evts = [];

                //Listen for global state changes
                evts.push(eventsService.on("appState.globalState.changed", function (e, args) {
                    if (args.key === "showNavigation") {
                        if (args.value === false) {
                            resetResize();
                        }
                        else {
                            enableResize();
                        }
                    }
                }));

                var resizeCallback = function(size) {
                    //set the global app state
                    appState.setGlobalState("isTablet", (size.width <= minScreenSize));
                    setTreeMode();
                };

                windowResizeListener.register(resizeCallback);

                //ensure to unregister from all events and kill jquery plugins
                scope.$on('$destroy', function () {
                    windowResizeListener.unregister(resizeCallback);
                    for (var e in evts) {
                        eventsService.unsubscribe(evts[e]);
                    }
                    var navInnerContainer = element.find(".navigation-inner-container");
                    navInnerContainer.resizable("destroy");
                });

                //init
                //set the global app state
                appState.setGlobalState("isTablet", ($(window).width() <= minScreenSize));
                setTreeMode();
            }
        };
    });
