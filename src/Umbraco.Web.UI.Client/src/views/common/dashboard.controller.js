/**
 * @ngdoc controller
 * @name DashboardController
 * @function
 * 
 * @description
 * Controls the dashboards of the application
 * 
 */
function DashboardController($scope, $routeParams) {
    $scope.name = $routeParams.section;
}
//register it
angular.module('umbraco').controller("DashboardController", DashboardController);
