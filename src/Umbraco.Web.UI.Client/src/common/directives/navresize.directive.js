/**
* @ngdoc directive
* @name umbraco.directives.directive:navResize
* @restrict A
 * 
 * @description
 * Handles how the navigation responds to window resizing and controls how the draggable resize panel works
**/
angular.module("umbraco.directives")
    .directive('navResize', function (appState, eventsService) {
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
                        element.find(".navigation-inner-container").resizable(
                        {
                            containment: $("#mainwrapper"),
                            autoHide: true,
                            handles: 'e',
                            resize: function(e, ui) {
                                var wrapper = $("#mainwrapper");
                                var contentPanel = $("#leftcolumn").next();
                                var apps = $("#applications");
                                var navOffeset = ui.element.next();
                                var leftPanelWidth = ui.element.width() + apps.width();

                                contentPanel.css({ left: leftPanelWidth });
                                navOffeset.css({ "margin-left": ui.element.outerWidth() });
                            },
                            stop: function(e, ui) {
                            }
                        });

                        resizeEnabled = true;
                    }
                }

                function resetResize() {
                    if (resizeEnabled) {
                        var navInnerContainer = element.find(".navigation-inner-container");

                        //kill the resize
                        navInnerContainer.resizable("destroy");

                        navInnerContainer.css("width", "");
                        $("#leftcolumn").next().css("left", "");
                        navInnerContainer.next().css("margin-left", "");

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

                $(window).bind("resize", function () {

                    //set the global app state
                    appState.setGlobalState("isTablet", ($(window).width() <= minScreenSize));

                    setTreeMode();
                });

                //ensure to unregister from all events and kill jquery plugins
                scope.$on('$destroy', function () {
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