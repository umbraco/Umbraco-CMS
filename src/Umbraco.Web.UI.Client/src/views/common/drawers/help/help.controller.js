(function () {
    "use strict";

    function HelpDrawerController($scope, $routeParams, $timeout, dashboardResource, localizationService, userService, eventsService, helpService, appState, tourService, $filter) {

        var vm = this;
        var evts = [];        

        vm.title = localizationService.localize("general_help");
        vm.subtitle = "Umbraco version" + " " + Umbraco.Sys.ServerVariables.application.version;
        vm.section = $routeParams.section;
        vm.tree = $routeParams.tree;
        vm.sectionName = "";
        vm.customDashboard = null;
        vm.tours = [];

        vm.closeDrawer = closeDrawer;
        vm.startTour = startTour;
        vm.getTourGroupCompletedPercentage = getTourGroupCompletedPercentage;
        vm.showTourButton = showTourButton;
        vm.showDocTypeTour = false;
        vm.docTypeTour = null;
        vm.startDoctypeTour = startDoctypeTour;
        vm.nodeName = '';
            
        function startTour(tour) {
            tourService.startTour(tour);
            closeDrawer();
        }

        function startDoctypeTour() {
            startTour(vm.docTypeTour);
        }

        function oninit() {

            tourService.getGroupedTours().then(function(groupedTours) {
                vm.tours = groupedTours;
                getTourGroupCompletedPercentage();
            });

            // load custom help dashboard
            dashboardResource.getDashboard("user-help").then(function (dashboard) {
                vm.customDashboard = dashboard;
            });

            if (!vm.section) {
                vm.section = "content";
            }

            setSectionName();

            userService.getCurrentUser().then(function (user) {
                
                vm.userType = user.userType;
                vm.userLang = user.locale;

                vm.hasAccessToSettings = _.contains(user.allowedSections, 'settings');                

                evts.push(eventsService.on("appState.treeState.changed", function (e, args) {                    
                    setDocTypeTour();
                    handleSectionChange();
                }));

                findHelp(vm.section, vm.tree, vm.userType, vm.userLang);

            });

            setDocTypeTour();
            
            // check if a tour is running - if it is open the matching group
            var currentTour = tourService.getCurrentTour();

            if (currentTour) {
                openTourGroup(currentTour.alias);
            }

        }

        function setDocTypeTour() {
            vm.showDocTypeTour = false;
            vm.docTypeTour = null;
            vm.nodeName = '';

            if (vm.section === 'content' && vm.tree === 'content') {
                var treeNode = appState.getTreeState('selectedNode');
                if (treeNode) {
                    tourService.getTourForDoctype(treeNode.metaData.contentType).then(function (data) {
                        if (data && data !== 'null') {
                            vm.docTypeTour = data;
                            vm.nodeName = treeNode.name;
                            vm.showDocTypeTour = true;
                        }
                    });  
                }                              
            }                     
        }

        function closeDrawer() {
            appState.setDrawerState("showDrawer", false);
        }

        function handleSectionChange() {
            $timeout(function () {
                if (vm.section !== $routeParams.section || vm.tree !== $routeParams.tree) {
                    
                    vm.section = $routeParams.section;
                    vm.tree = $routeParams.tree;

                    setSectionName();
                    findHelp(vm.section, vm.tree, vm.userType, vm.userLang);
                    setDocTypeTour();
                }
            });
        }

        function findHelp(section, tree, usertype, userLang) {

            if (vm.hasAccessToSettings) {
                helpService.getContextHelpForPage(section, tree).then(function (topics) {
                    vm.topics = topics;
                });
            }
            

            var rq = {};
            rq.section = vm.section;
            rq.usertype = usertype;
            rq.lang = userLang;

            if ($routeParams.url) {
                rq.path = decodeURIComponent($routeParams.url);

                if (rq.path.indexOf(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath) === 0) {
                    rq.path = rq.path.substring(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath.length);
                }

                if (rq.path.indexOf(".aspx") > 0) {
                    rq.path = rq.path.substring(0, rq.path.indexOf(".aspx"));
                }

            } else {
                rq.path = rq.section + "/" + $routeParams.tree + "/" + $routeParams.method;
            }


            if (vm.hasAccessToSettings) {
                helpService.findVideos(rq).then(function (videos) {
                    vm.videos = videos;
                });
            }                       
        }

        function setSectionName() {
            // Get section name
            var languageKey = "sections_" + vm.section;
            localizationService.localize(languageKey).then(function (value) {
                vm.sectionName = value;
            });
        }

        function showTourButton(index, tourGroup) {
            if(index !== 0) {
                var prevTour = tourGroup.tours[index - 1];
                if(prevTour.completed) {
                    return true;
                }
            } else {
                return true;
            }
        }

        function openTourGroup(tourAlias) {
            angular.forEach(vm.tours, function (group) {
                angular.forEach(group, function (tour) {
                    if (tour.alias === tourAlias) {
                        group.open = true;
                    }
                });
            });
        }

        function getTourGroupCompletedPercentage() {
            // Finding out, how many tours are completed for the progress circle
            angular.forEach(vm.tours, function(group){
                var completedTours = 0;
                angular.forEach(group.tours, function(tour){
                    if(tour.completed) {
                        completedTours++;
                    }
                });
                group.completedPercentage = Math.round((completedTours/group.tours.length)*100);
            });
        }

        evts.push(eventsService.on("appState.tour.complete", function (event, tour) {
            tourService.getGroupedTours().then(function(groupedTours) {
                vm.tours = groupedTours;
                openTourGroup(tour.alias);
                getTourGroupCompletedPercentage();
            });
        }));
           
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

        oninit();

    }

    angular.module("umbraco").controller("Umbraco.Drawers.Help", HelpDrawerController);
})();
