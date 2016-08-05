angular.module("umbraco").controller("Umbraco.Dashboard.RedirectUrlsController", function($scope, redirectUrlsResource, notificationsService, $q) {
    //...todo
    //search by url or url part
    //search by domain
    //display domain in dashboard results?

    //used to cancel any request in progress if another one needs to take it's place
    var canceler = null;

    $scope.dashboard = {
        searchTerm: "",
        loading: false,
        UrlTrackerDisabled: false
    };

    $scope.pagination = {
        pageIndex: 0,
        pageNumber: 1,
        totalPages: 1,
        pageSize: 20
    };

    function activate() {
        $scope.search();
    }

    $scope.goToPage = function(pageNumber) {
        $scope.pagination.pageIndex = pageNumber - 1;
        $scope.pagination.pageNumber = pageNumber;
        $scope.search();
    };

    $scope.search = function() {

        $scope.dashboard.loading = true;

        var searchTerm = $scope.dashboard.searchTerm;
        if (searchTerm === undefined) {
            searchTerm = "";
        }

        redirectUrlsResource.searchRedirectUrls(searchTerm, $scope.pagination.pageIndex, $scope.pagination.pageSize).then(function(response) {

            $scope.redirectUrls = response.SearchResults;

            // update pagination
            $scope.pagination.pageIndex = response.CurrentPage;
            $scope.pagination.pageNumber = response.CurrentPage + 1;
            $scope.pagination.totalPages = response.PageCount;

            // Set enable/disable state for url tracker
            $scope.dashboard.UrlTrackerDisabled = response.UrlTrackerDisabled;

            angular.forEach($scope.redirectUrls, function(redirect) {
                redirectUrlsResource.getPublishedUrl(redirect.ContentId).then(function(response) {
                    redirect.ContentUrl = response;
                }, function(error) {
                    notificationsService.error("Redirect Url Error!", "Failed to get published url for " + redirect.Url);
                });
            });

            $scope.dashboard.loading = false;

        });
    };

    $scope.removeRedirect = function(redirectToDelete) {

        redirectUrlsResource.deleteRedirectUrl(redirectToDelete.Id).then(function() {

            var index = $scope.redirectUrls.indexOf(redirectToDelete);
            $scope.redirectUrls.splice(index, 1);
            notificationsService.success("Redirect Url Removed!", "Redirect Url " + redirectToDelete.Url + " has been deleted");

        }, function(error) {

            notificationsService.error("Redirect Url Error!", "Redirect Url " + redirectToDelete.Url + " was not deleted");

        });

    };

    $scope.disableUrlTracker = function() {
        var toggleConfirm = confirm("Are you sure you want to disable the URL tracker?");
        if (toggleConfirm) {

            redirectUrlsResource.toggleUrlTracker(true).then(function() {
                activate();
                notificationsService.success("URL Tracker has now been disabled");
            }, function(error) {
                notificationsService.warning("Error disabling the URL Tracker, more information can be found in your log file.");
            });

        }
    };

    $scope.enableUrlTracker = function() {
        redirectUrlsResource.toggleUrlTracker(false).then(function() {
            activate();
            notificationsService.success("URL Tracker has now been enabled");
        }, function(error) {
            notificationsService.warning("Error enabling the URL Tracker, more information can be found in your log file.");
        });
    };

    var filterDebounced = _.debounce(function(e) {

        $scope.$apply(function() {

            //a canceler exists, so perform the cancelation operation and reset
            if (canceler) {
                canceler.resolve();
                canceler = $q.defer();
            } else {
                canceler = $q.defer();
            }

            $scope.search();

        });

    }, 200);

    $scope.filter = function() {
        filterDebounced();
    };

    activate();

});
