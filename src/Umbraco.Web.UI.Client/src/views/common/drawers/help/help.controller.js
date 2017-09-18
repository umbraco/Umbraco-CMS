(function () {
    "use strict";

    function HelpDrawerController($scope, $routeParams, $timeout, dashboardResource, localizationService, userService, eventsService, helpService, appState) {

        var vm = this;
        var evts = [];

        vm.title = localizationService.localize("general_help");
        vm.subtitle = "Umbraco version" + " " + Umbraco.Sys.ServerVariables.application.version;
        vm.section = $routeParams.section;
        vm.tree = $routeParams.tree;
        vm.sectionName = "";
        vm.customDashboard = null;

        vm.closeDrawer = closeDrawer;

        function oninit() {

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

                evts.push(eventsService.on("appState.treeState.changed", function (e, args) {
                    handleSectionChange();
                }));

                findHelp(vm.section, vm.tree, vm.usertype, vm.userLang);

            });

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
                    findHelp(vm.section, vm.tree, vm.usertype, vm.userLang);

                }
            });
        }

        function findHelp(section, tree, usertype, userLang) {
            
            helpService.getContextHelpForPage(section, tree).then(function (topics) {
                vm.topics = topics;
            });

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

            helpService.findVideos(rq).then(function(videos){
    	        vm.videos = videos;
            });
            
        }

        function setSectionName() {
            // Get section name
            var languageKey = "sections_" + vm.section;
            localizationService.localize(languageKey).then(function (value) {
                vm.sectionName = value;
            });
        }

        oninit();
           
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });

    }

    angular.module("umbraco").controller("Umbraco.Drawers.Help", HelpDrawerController);
})();
