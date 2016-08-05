angular.module("umbraco").controller("Umbraco.Dashboard.RedirectUrlsController", function($scope, $http, angularHelper, notificationsService, entityResource, $routeParams, $q) {
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

        $http.get("backoffice/api/RedirectUrlManagement/SearchRedirectUrls/?searchTerm=" + searchTerm + "&page=" + $scope.pagination.pageIndex + "&pageSize=" + $scope.pagination.pageSize).then(function(response) {

            console.log(response);

            $scope.redirectUrls = response.data.SearchResults;

            // update pagination
            $scope.pagination.pageIndex = response.data.CurrentPage;
            $scope.pagination.pageNumber = response.data.CurrentPage + 1;
            $scope.pagination.totalPages = response.data.PageCount;

            // Set enable/disable state for url tracker
            $scope.dashboard.UrlTrackerDisabled = response.data.UrlTrackerDisabled;

            angular.forEach($scope.redirectUrls, function(item) {
                $http.get("backoffice/api/RedirectUrlManagement/GetPublishedUrl/?id=" + item.ContentId).then(function(response) {
                    item.ContentUrl = response.data;
                });
            });

            $scope.dashboard.loading = false;

        });
    };

    $scope.removeRedirect = function(redirectToDelete) {
        $http.post("backoffice/api/RedirectUrlManagement/DeleteRedirectUrl/" + redirectToDelete.Id).then(function(response) {
            if (response.status === 200) {

                var index = $scope.redirectUrls.indexOf(redirectToDelete);
                $scope.redirectUrls.splice(index, 1);

                notificationsService.success("Redirect Url Removed!", "Redirect Url " + redirectToDelete.Url + " has been deleted");
            } else {
                notificationsService.warning("Redirect Url Error!", "Redirect Url " + redirectToDelete.Url + " was not deleted");
            }
        });
    };

    $scope.disableUrlTracker = function() {
        var toggleConfirm = confirm("Are you sure you want to disable the URL tracker?");
        if (toggleConfirm) {
            $http.post("backoffice/api/RedirectUrlManagement/ToggleUrlTracker/?disable=true").then(function(response) {
                if (response.status === 200) {
                    notificationsService.success("URL Tracker has now been disabled");
                    activate();
                } else {
                    notificationsService.warning("Error disabling the URL Tracker, more information can be found in your log file.");
                }
            });
        }
    };

    $scope.enableUrlTracker = function() {
        if (toggleConfirm) {
            $http.post("backoffice/api/RedirectUrlManagement/ToggleUrlTracker/?disable=false").then(function(response) {
                if (response.status === 200) {
                    notificationsService.success("URL Tracker has now been enabled");
                    activate();
                } else {
                    notificationsService.warning("Error enabling the URL Tracker, more information can be found in your log file.");
                }
            });
        }
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
