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

            //setup scope vars
            scope.sections = [];
            scope.visibleSections = 0;
            scope.currentSection = appState.getSectionState("currentSection");
            scope.showTray = false;
            scope.stickyNavigation = appState.getGlobalState("stickyNavigation");

            function loadSections() {
                sectionService.getSectionsForUser()
                    .then(function (result) {
                        scope.sections = result;
                        scope.visibleSections = scope.sections.length;

                        // store the width of each section so we can hide/show them based on browser width 
                        // we store them because the sections get removed from the dom and then we 
                        // can't tell when to show them gain
                        $timeout(function () {
                            $("#applications .sections li:not(:last)").each(function (index) {
                                sectionItemsWidth.push($(this).outerWidth());
                            });
                        });
                        calculateWidth();
                    });
            }

            function calculateWidth() {
                $timeout(function () {
                    //total width minus room for avatar, search, and help icon
                    var containerWidth = $(".umb-app-header").outerWidth() - $(".umb-app-header__actions").outerWidth();
                    var trayToggleWidth = $("#applications .sections li.expand").outerWidth();
                    var sectionsWidth = 0;
                    
                    // detect how many sections we can show on the screen
                    for (var i = 0; i < sectionItemsWidth.length; i++) {
                        var sectionItemWidth = sectionItemsWidth[i];
                        sectionsWidth += sectionItemWidth;

                        if (sectionsWidth + trayToggleWidth > containerWidth) {
                            scope.visibleSections =  i;
                            return;
                        }
                    }

                    scope.visibleSections = scope.sections.length;
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

            scope.currentSectionInOverflow = function () {
                var currentSection = scope.sections.filter(s => s.alias === scope.currentSection);

                return currentSection.length > 0 && scope.sections.indexOf(currentSection[0]) > scope.visibleSections - 1;
            };

            loadSections();

        }
    };
}

angular.module('umbraco.directives').directive("umbSections", sectionsDirective);
