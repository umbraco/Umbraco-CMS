(function() {
    "use strict";

    function RedirectUrlsController($scope, $q, redirectUrlsResource, notificationsService, localizationService, eventsService, overlayService) {
        // TODO: search by url or url part
        // TODO: search by domain
        // TODO: display domain in dashboard results?

        // used to cancel any request in progress if another one needs to take it's place
        var vm = this;
        var canceler = null;

        vm.dashboard = {
            searchTerm: "",
            loading: false,
            urlTrackerDisabled: false,
            userIsAdmin: false
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
            return redirectUrlsResource.getEnableState().then(function (response) {
                vm.dashboard.urlTrackerDisabled = response.enabled !== true;
                vm.dashboard.userIsAdmin = response.userIsAdmin;
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

        function disableUrlTracker(event) {

            const dialog = {
                view: "views/dashboard/content/overlays/disable.html",
                submitButtonLabel: "Disable",
                submitButtonLabelKey: "actions_disable",
                submitButtonStyle:"danger",
                submit: function (model) {
                    performDisable();
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            localizationService.localize("redirectUrls_disableUrlTracker").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });

            event.preventDefault()
            event.stopPropagation();
        }

        function removeRedirect(redirect, event) {

            const dialog = {
                view: "views/dashboard/content/overlays/delete.html",
                redirect: redirect,
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
                submitButtonStyle: "danger",
                submit: function (model) {
                    performDelete(model.redirect);
                    overlayService.close();
                },
                close: function () {
                    overlayService.close();
                }
            };

            localizationService.localize("general_delete").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });

            event.preventDefault()
            event.stopPropagation();
        }

        function performDisable() {

            redirectUrlsResource.toggleUrlTracker(true).then(function () {
                activate();
                localizationService.localize("redirectUrls_disabledConfirm").then(function (value) {
                    notificationsService.success(value);
                });
            }, function (error) {
                localizationService.localize("redirectUrls_disableError").then(function (value) {
                    notificationsService.warning(value);
                });
            });
        }

        function performDelete(redirect) {
            redirect.deleteButtonState = "busy";

            redirectUrlsResource.deleteRedirectUrl(redirect.redirectId).then(function () {

                // emit event
                var args = { redirect: redirect };
                eventsService.emit("editors.redirects.redirectDeleted", args);

                // remove from list
                var index = vm.redirectUrls.indexOf(redirect);
                vm.redirectUrls.splice(index, 1);

                localizationService.localize("redirectUrls_redirectRemoved").then(function (value) {
                    notificationsService.success(value);
                });

                // check if new redirects needs to be loaded
                if (vm.redirectUrls.length === 0 && vm.pagination.totalPages > 1) {

                    // if we are not on the first page - get records from the previous
                    if (vm.pagination.pageIndex > 0) {
                        vm.pagination.pageIndex = vm.pagination.pageIndex - 1;
                        vm.pagination.pageNumber = vm.pagination.pageNumber - 1;
                    }

                    search();
                }
            }, function (error) {
                redirect.deleteButtonState = "error";

                localizationService.localize("redirectUrls_redirectRemoveError").then(function (value) {
                    notificationsService.error(value);
                });
            });
        }

        function enableUrlTracker() {
            redirectUrlsResource.toggleUrlTracker(false).then(function () {
                activate();
                localizationService.localize("redirectUrls_enabledConfirm").then(function (value) {
                    notificationsService.success(value);
                });
            }, function (error) {
                localizationService.localize("redirectUrls_enableError").then(function (value) {
                    notificationsService.warning(value);
                });
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
