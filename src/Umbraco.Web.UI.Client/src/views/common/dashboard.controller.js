/**
 * @ngdoc controller
 * @name Umbraco.DashboardController
 * @function
 * 
 * @description
 * Controls the dashboards of the application
 * 
 */
 
function DashboardController($scope, $routeParams, dashboardResource, localizationService) {

   $scope.page = {};
   $scope.page.nameLocked = true;
   $scope.page.loading = true;

    $scope.dashboard = {};
    localizationService.localize("sections_" + $routeParams.section).then(function(name){
    	$scope.dashboard.name = name;
    });
    
    dashboardResource.getDashboard($routeParams.section).then(function(tabs){
      $scope.dashboard.tabs = tabs;
      if ($scope.dashboard.tabs) {
        _.each($scope.dashboard.tabs, function (tab) {
          if (tab.label.startsWith("@")) {
            localizationService.localize(tab.label.substring(1)).then(function (value) {
              if (value !== "") {
                tab.label = value;
              }
            });
          }
        });
      }
      $scope.page.loading = false;
    });
}


//register it
angular.module('umbraco').controller("Umbraco.DashboardController", DashboardController);
