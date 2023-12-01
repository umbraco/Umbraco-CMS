/**
 * @ngdoc controller
 * @name Umbraco.DashboardController
 * @function
 * 
 * @description
 * Controls the dashboards of the application
 * 
 */

function DashboardController($scope, $q, $routeParams, $location, dashboardResource, localizationService) {

    const DASHBOARD_QUERY_PARAM = 'dashboard';
    const promises = [];

    const vm = this;

    vm.page = {};
    vm.page.nameLocked = true;
    vm.page.loading = true;

    vm.changeTab = changeTab;

    $scope.dashboard = {};

    promises.push(localizationService.localize("sections_" + $routeParams.section).then(name => {
    	$scope.dashboard.name = name;
    }));

    promises.push(dashboardResource.getDashboard($routeParams.section).then(tabs => {
        $scope.dashboard.tabs = tabs;

        if ($scope.dashboard.tabs && $scope.dashboard.tabs.length > 0) {
            initActiveTab();
        }
    }));

    $q.all(promises).then(() => {
        vm.page.loading = false;
    });

    function changeTab(tab) {
        if ($scope.dashboard.tabs && $scope.dashboard.tabs.length > 0) {
            $scope.dashboard.tabs.forEach(tab => {
                tab.active = false;
            });
        }

        tab.active = true;
        $location.search(DASHBOARD_QUERY_PARAM, tab.alias);
    }

    function initActiveTab() {
        // Check the query parameter for a dashboard alias
        const dashboardAlias = $location.search()[DASHBOARD_QUERY_PARAM];
        const dashboardIndex = $scope.dashboard.tabs.findIndex(tab => tab.alias === dashboardAlias);
        const showDefaultDashboard = dashboardIndex === -1;

        // Set the first dashboard to active if there is no query parameter or we can't find a matching dashboard for the alias
        const activeIndex = showDefaultDashboard ? 0 : dashboardIndex;

        const tab = $scope.dashboard.tabs[activeIndex];

        tab.active = true;
        if (!showDefaultDashboard) {
            $location.search(DASHBOARD_QUERY_PARAM, tab.alias);
        }
    }
}

// Register it
angular.module('umbraco').controller("Umbraco.DashboardController", DashboardController);
