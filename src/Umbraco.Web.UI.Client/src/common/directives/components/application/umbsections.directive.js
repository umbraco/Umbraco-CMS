/**
* @ngdoc directive
* @name umbraco.directives.directive:umbSections
* @restrict E
**/
function sectionsDirective($timeout, $window, navigationService, treeService, sectionService, appState, eventsService, $location, historyService) {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/components/application/umb-sections.html',
        link: function (scope, element, attr, ctrl) {

            var sectionItemsWidth = [];
            var evts = [];
            var maxSections = 8;

            //setup scope vars
            scope.maxSections = maxSections;
            scope.overflowingSections = 0;
            scope.sections = [];
            scope.currentSection = appState.getSectionState("currentSection");
            scope.showTray = false; //appState.getGlobalState("showTray");
            scope.stickyNavigation = appState.getGlobalState("stickyNavigation");
            scope.needTray = false;

            function loadSections() {
                sectionService.getSectionsForUser()
                    .then(function (result) {
                        scope.sections = result;
                        // store the width of each section so we can hide/show them based on browser width 
                        // we store them because the sections get removed from the dom and then we 
                        // can't tell when to show them gain
                        $timeout(function () {
                            $("#applications .sections li").each(function (index) {
                                sectionItemsWidth.push($(this).outerWidth());
                            });
                        });
                        calculateWidth();
                    });
            }

            function calculateWidth() {
                $timeout(function () {
                    //total width minus room for avatar, search, and help icon
                    var windowWidth = $(window).width()-150;
                    var sectionsWidth = 0;
                    scope.totalSections = scope.sections.length;
                    scope.maxSections = maxSections;
                    scope.overflowingSections = scope.maxSections - scope.totalSections;
                    scope.needTray = scope.sections.length > scope.maxSections;

                    // detect how many sections we can show on the screen
                    for (var i = 0; i < sectionItemsWidth.length; i++) {
                        var sectionItemWidth = sectionItemsWidth[i];
                        sectionsWidth += sectionItemWidth;

                        if (sectionsWidth > windowWidth) {
                            scope.needTray = true;
                            scope.maxSections = i - 1;
                            scope.overflowingSections = scope.maxSections - scope.totalSections;
                            break;
                        }
                    }
                });
            }

            //Listen for global state changes
            evts.push(eventsService.on("appState.globalState.changed", function (e, args) {
                if (args.key === "showTray") {
                    scope.showTray = args.value;
                }
                if (args.key === "stickyNavigation") {
                    scope.stickyNavigation = args.value;
                }
            }));

            evts.push(eventsService.on("appState.sectionState.changed", function (e, args) {
                if (args.key === "currentSection") {
                    scope.currentSection = args.value;
                }
            }));

            evts.push(eventsService.on("app.reInitialize", function (e, args) {
                //re-load the sections if we're re-initializing (i.e. package installed)
                loadSections();
            }));

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

            //on page resize
            window.onresize = calculateWidth;

            scope.sectionClick = function (event, section) {

                if (event.ctrlKey ||
                    event.shiftKey ||
                    event.metaKey || // apple
                    (event.button && event.button === 1) // middle click, >IE9 + everyone else
                ) {
                    return;
                }

                navigationService.hideSearch();
                navigationService.showTree(section.alias);

                //in some cases the section will have a custom route path specified, if there is one we'll use it
                if (section.routePath) {
                    $location.path(section.routePath);
                }
                else {
                    var lastAccessed = historyService.getLastAccessedItemForSection(section.alias);
                    var path = lastAccessed != null ? lastAccessed.link : section.alias;
                    $location.path(path);
                }
                navigationService.clearSearch();

            };

            scope.sectionDblClick = function (section) {
                navigationService.reloadSection(section.alias);
            };

            scope.trayClick = function () {
                if (appState.getGlobalState("showTray") === true) {
                    navigationService.hideTray();
                } else {
                    navigationService.showTray();
                }
            };

            loadSections();

        }
    };
}

angular.module('umbraco.directives').directive("umbSections", sectionsDirective);
