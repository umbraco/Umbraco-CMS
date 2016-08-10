(function() {
    "use strict";

    function RedirectUrlsController($scope, redirectUrlsResource, notificationsService, $q) {
        //...todo
        //search by url or url part
        //search by domain
        //display domain in dashboard results?

        //used to cancel any request in progress if another one needs to take it's place
        var vm = this;
        var canceler = null;

        vm.dashboard = {
            searchTerm: "",
            loading: false,
            urlTrackerDisabled: false
        };

        vm.pagination = {
            pageIndex: 0,
            pageNumber: 1,
            totalPages: 1,
            pageSize: 20
        };

        vm.goToPage = goToPage;
        vm.search = search;
        vm.removeRedirect = removeRedirect;
        vm.disableUrlTracker = disableUrlTracker;
        vm.enableUrlTracker = enableUrlTracker;
        vm.filter = filter;
        vm.checkEnabled = checkEnabled;

        function activate() {            
            vm.checkEnabled().then(function() {
                vm.search();
            });
        }

        function checkEnabled() {
            vm.dashboard.loading = true;
            return redirectUrlsResource.isEnabled().then(function (response) {
                vm.dashboard.urlTrackerDisabled = response !== "true";
                vm.dashboard.loading = false;
            });
        }

        function goToPage(pageNumber) {
            vm.pagination.pageIndex = pageNumber - 1;
            vm.pagination.pageNumber = pageNumber;
            vm.search();
        }

        function search() {

            vm.dashboard.loading = true;

            var searchTerm = vm.dashboard.searchTerm;
            if (searchTerm === undefined) {
                searchTerm = "";
            }

            redirectUrlsResource.searchRedirectUrls(searchTerm, vm.pagination.pageIndex, vm.pagination.pageSize).then(function(response) {

                vm.redirectUrls = response.searchResults;

                // update pagination
                vm.pagination.pageIndex = response.currentPage;
                vm.pagination.pageNumber = response.currentPage + 1;
                vm.pagination.totalPages = response.pageCount;

                vm.dashboard.loading = false;

            });
        }

        function removeRedirect(redirectToDelete) {
            var toggleConfirm = confirm('Are you sure you want to remove the redirect from ' + '"' + redirectToDelete.originalUrl + '"' + " to " + '"' + redirectToDelete.destinationUrl + '"' + " ?");

            if (toggleConfirm) {
                redirectUrlsResource.deleteRedirectUrl(redirectToDelete.redirectId).then(function () {

                    var index = vm.redirectUrls.indexOf(redirectToDelete);
                    vm.redirectUrls.splice(index, 1);
                    notificationsService.success("Redirect Url Removed!", "Redirect Url has been deleted");

                }, function(error) {
                    notificationsService.error("Redirect Url Error!", "Redirect Url was not deleted");
                });
            }
        }

        function disableUrlTracker() {
            var toggleConfirm = confirm("Are you sure you want to disable the URL tracker?");
            if (toggleConfirm) {

                redirectUrlsResource.toggleUrlTracker(true).then(function() {
                    activate();
                    notificationsService.success("URL Tracker has now been disabled");
                }, function(error) {
                    notificationsService.warning("Error disabling the URL Tracker, more information can be found in your log file.");
                });

            }
        }

        function enableUrlTracker() {
            redirectUrlsResource.toggleUrlTracker(false).then(function() {
                activate();
                notificationsService.success("URL Tracker has now been enabled");
            }, function(error) {
                notificationsService.warning("Error enabling the URL Tracker, more information can be found in your log file.");
            });
        }

        var filterDebounced = _.debounce(function(e) {

            $scope.$apply(function() {

                //a canceler exists, so perform the cancelation operation and reset
                if (canceler) {
                    canceler.resolve();
                    canceler = $q.defer();
                } else {
                    canceler = $q.defer();
                }

                vm.search();

            });

        }, 200);

        function filter() {
            vm.dashboard.loading = true;
            filterDebounced();
        }

        activate();

    }

    angular.module("umbraco").controller("Umbraco.Dashboard.RedirectUrlsController", RedirectUrlsController);
})();
