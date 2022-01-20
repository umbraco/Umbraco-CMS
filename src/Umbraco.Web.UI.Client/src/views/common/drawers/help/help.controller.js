
(function () {
    "use strict";

    function HelpDrawerController($scope, $routeParams, $timeout, dashboardResource, localizationService, userService, eventsService, helpService, appState, tourService, $filter, editorState, notificationsService, currentUserResource, platformService) {

        var vm = this;
        var evts = [];
        vm.title = "";
        vm.subtitle = "Umbraco version" + " " + Umbraco.Sys.ServerVariables.application.version;
        vm.section = $routeParams.section;
        vm.tree = $routeParams.tree;
        vm.sectionName = "";
        vm.customDashboard = null;
        vm.tours = [];
        vm.systemInfoDisplay = false;
        vm.labels = {};
        vm.labels.copiedSuccessInfo = "";
        vm.labels.copySuccessStatus = "";
        vm.labels.copiedErrorInfo = "";
        vm.labels.copyErrorStatus = "";


        vm.closeDrawer = closeDrawer;
        vm.startTour = startTour;
        vm.getTourGroupCompletedPercentage = getTourGroupCompletedPercentage;
        vm.showTourButton = showTourButton;
        vm.copyInformation = copyInformation;

        vm.showDocTypeTour = false;
        vm.docTypeTours = [];
        vm.systemInfo = [];
        vm.nodeName = '';

        function startTour(tour) {
            tourService.startTour(tour);
            closeDrawer();
        }

        function oninit() {

            // set title
            localizationService.localize("general_help").then(function(data){
                vm.title = data;
            });
            //Set help dashboard messages
          var labelKeys = [
            "general_help",
            "speechBubbles_copySuccessMessage",
            "general_success",
            "speechBubbles_cannotCopyInformation",
            "general_error"
          ];
          localizationService.localizeMany(labelKeys).then(function(resp){
            [
              vm.title,
              vm.labels.copiedSuccessInfo,
              vm.labels.copySuccessStatus,
              vm.labels.copiedErrorInfo,
              vm.labels.copyErrorStatus
            ] = resp;
          });

            currentUserResource.getUserData().then(function(systemInfo){
              vm.systemInfo = systemInfo;
              let browserInfo = platformService.getBrowserInfo();
              if(browserInfo != null){
                vm.systemInfo.push({name :"Browser", data: browserInfo.name + " " + browserInfo.version});
              }
              vm.systemInfo.push({name :"Browser OS", data: getPlatform()});
            } );
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
                    handleSectionChange();
                }));

                evts.push(eventsService.on("editorState.changed",
                    function (e, args) {
                        setDocTypeTour(args.entity);
                    }));

                findHelp(vm.section, vm.tree, vm.userType, vm.userLang);

            });

            setDocTypeTour(editorState.getCurrent());

            // check if a tour is running - if it is open the matching group
            var currentTour = tourService.getCurrentTour();

            if (currentTour) {
                openTourGroup(currentTour.alias);
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
            vm.tours.forEach(function (group) {
                group.tours.forEach(function (tour) {
                    if (tour.alias === tourAlias) {
                        group.open = true;
                    }
                });
            });
        }

        function getTourGroupCompletedPercentage() {
            // Finding out, how many tours are completed for the progress circle
            vm.tours.forEach(function(group){
                var completedTours = 0;
                group.tours.forEach(function(tour){
                    if(tour.completed) {
                        completedTours++;
                    }
                });
                group.completedPercentage = Math.round((completedTours/group.tours.length)*100);
            });
        }

        function setDocTypeTour(node) {
            vm.showDocTypeTour = false;
            vm.docTypeTours = [];
            vm.nodeName = '';

            if (vm.section === 'content' && vm.tree === 'content') {

                if (node) {
                    tourService.getToursForDoctype(node.contentTypeAlias).then(function (data) {
                        if (data && data.length > 0) {
                            vm.docTypeTours = data;
                            var currentVariant = _.find(node.variants, (x) => x.active);
                            vm.nodeName = currentVariant.name;
                            vm.showDocTypeTour = true;
                        }
                    });
                }
            }
        }
        function copyInformation(){
          //Write start and end text for table formatting in github issues
          let copyStartText = "<html>\n<body>\n<!--StartFragment-->\n\nCategory | Data\n-- | --\n";
          let copyEndText = "\n<!--EndFragment-->\n</body>\n</html>";

          let copyText = copyStartText;
          vm.systemInfo.forEach(function (info){
            copyText += info.name + " | " + info.data + "\n";
          });

          copyText += copyEndText;

          // Check if copyText is only start + end text
          // if it is something went wrong and we will not copy to clipboard
          let emptyCopyText = copyStartText + copyEndText;
          if(copyText !== emptyCopyText) {
            notificationsService.success(vm.labels.copySuccessStatus, vm.labels.copiedSuccessInfo);
            navigator.clipboard.writeText(copyText);
          }
          else {
            notificationsService.error(vm.labels.copyErrorStatus, vm.labels.copiedErrorInfo);
          }
        }

        function getPlatform() {
          return window.navigator.platform;
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
