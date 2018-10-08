function publishedStatusController($scope, umbRequestHelper, $log, $http, $q, $timeout) {

    // note: must defined base url in BackOfficeController

    umbRequestHelper.resourcePromise(
        $http.get(umbRequestHelper.getApiUrl('publishedStatusBaseUrl', 'GetPublishedStatusUrl')),
        'Failed to get published status url')
    .then(function (result) {

        //result = 'views/dashboard/developer/nucache.html'
        $scope.includeUrl = result;
    });

}
angular.module("umbraco").controller("Umbraco.Dashboard.PublishedStatusController", publishedStatusController);