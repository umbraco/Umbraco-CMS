function facadeStatusController($scope, umbRequestHelper, $log, $http, $q, $timeout) {

    // note: must defined 'facaStatusBaseUrl' in BackOfficeController

    umbRequestHelper.resourcePromise(
        $http.get(umbRequestHelper.getApiUrl('facadeStatusBaseUrl', 'GetFacadeStatusUrl')),
        'Failed to get facade status url')
    .then(function (result) {
        $scope.includeUrl = angular.fromJson(result);
    });

    //$scope.includeUrl = 'views/dashboard/developer/xmldataintegrityreport.html';

}
angular.module("umbraco").controller("Umbraco.Dashboard.FacadeStatusController", facadeStatusController);