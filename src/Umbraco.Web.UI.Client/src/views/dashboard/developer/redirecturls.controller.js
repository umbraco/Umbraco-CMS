(function() {
    "use strict";

    function RedirectUrlsController($scope, redirectUrlsResource, notificationsService, localizationService, $q) {
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
        vm.addRedirect = addRedirect;
        vm.openContentPickerOverlay = openContentPickerOverlay;
        vm.removeRedirect = removeRedirect;
        vm.disableUrlTracker = disableUrlTracker;
        vm.enableUrlTracker = enableUrlTracker;
        vm.filter = filter;
        vm.checkEnabled = checkEnabled;

        vm.status = {
            adding: false,
            readyToAdd: false
        };
        vm.data = {
            redirectToSelection: [],
            redirectFromUrl: ""
        }
        vm.removeSelection = removeSelection;

        function removeSelection() {
            console.log(vm.data.redirectToSelection);
            vm.data.redirectToSelection = [];
        }
        function openContentPickerOverlay() {
            console.log("hi");
            vm.contentPickerOverlay = {
                multiPicker: false,
                view: "contentpicker",
                show: true,
                submit: function (model) {
                    console.log(model);
                    angular.forEach(model.selection,
                        function(entity) {
                            setEntityUrl(entity);
                        });
                    
                    vm.data.redirectToSelection = model.selection;
                    vm.status.readyToAdd = vm.data.redirectFromUrl.length > 0 && vm.data.redirectToSelection.length > 0;
                    vm.contentPickerOverlay.show = false;
                    vm.contentPickerOverlay = null;

                },
                close: function (oldModel) {
                    vm.contentPickerOverlay.show = false;
                    vm.contentPickerOverlay = null;
                }
            }

        };

        function setEntityUrl(entity) {

            // get url for content and media items
            if (entityType !== "Member") {
                entityResource.getUrl(entity.id, entityType).then(function(data) {
                    // update url  
                    if (entity.trashed) {
                        item.url = localizationService.dictionary.general_recycleBin;
                    } else {
                        item.url = data;
                    }

                });
            }
        }

        function activate() {            
            vm.checkEnabled().then(function() {
                vm.search();
                vm.data.redirectFromUrl = "";
                vm.data.redirectToSelection = [];
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

        function addRedirect() {
            vm.addRedirectOverlay = {
                show: true,
                view: "treepicker",
                multiPicker: false,
                entityType: "document",
                filterCssClass: "not-allowed not-published",
                startNodeId: null,
                callback: function (data) {
                    console.log(data);
                },
                treeAlias: "content",
                section: "content",
                idType: "udi"
            
            };
            vm.addRedirectOverlay.submit = function(model) {

                console.log('submitted', model);
                vm.addRedirectOverlay.close();
            }

            vm.addRedirectOverlay.close = function() {
                vm.addRedirectOverlay.show = false;
                vm.addRedirectOverlay = null;
            }
        }

        function removeRedirect(redirectToDelete) {
            localizationService.localize("redirectUrls_confirmRemove", [redirectToDelete.originalUrl, redirectToDelete.destinationUrl]).then(function (value) {
                var toggleConfirm = confirm(value);

                if (toggleConfirm) {
                    redirectUrlsResource.deleteRedirectUrl(redirectToDelete.redirectId).then(function () {

                        var index = vm.redirectUrls.indexOf(redirectToDelete);
                        vm.redirectUrls.splice(index, 1);
                        notificationsService.success(localizationService.localize("redirectUrls_redirectRemoved"));

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
                        notificationsService.error(localizationService.localize("redirectUrls_redirectRemoveError"));
                    });
                }
            });
        }

        function disableUrlTracker() {
            localizationService.localize("redirectUrls_confirmDisable").then(function(value) {
                var toggleConfirm = confirm(value);
                if (toggleConfirm) {

                    redirectUrlsResource.toggleUrlTracker(true).then(function () {
                        activate();
                        notificationsService.success(localizationService.localize("redirectUrls_disabledConfirm"));
                    }, function (error) {
                        notificationsService.warning(localizationService.localize("redirectUrls_disableError"));
                    });

                }
            });
        }

        function enableUrlTracker() {
            redirectUrlsResource.toggleUrlTracker(false).then(function() {
                activate();
                notificationsService.success(localizationService.localize("redirectUrls_enabledConfirm"));
            }, function(error) {
                notificationsService.warning(localizationService.localize("redirectUrls_enableError"));
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
