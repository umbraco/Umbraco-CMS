function publishedStatusController($scope, $http, umbRequestHelper) {

    var vm = this;

    // note: must defined base url in BackOfficeController
    umbRequestHelper.resourcePromise(
        $http.get(umbRequestHelper.getApiUrl('publishedStatusBaseUrl', 'GetPublishedStatusUrl')),
        'Failed to get published status url')
    .then(function (result) {
        vm.includeUrl = result;
    });

}
angular.module("umbraco").controller("Umbraco.Dashboard.PublishedStatusController", publishedStatusController);
