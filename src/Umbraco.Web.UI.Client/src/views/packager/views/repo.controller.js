(function () {
    "use strict";

    function PackagesRepoController($scope, $route, $location, $timeout, ourPackageRepositoryResource, $q, packageResource, localStorageService, localizationService) {

        var vm = this;

        vm.packageViewState = "packageList";
        vm.categories = [];
        vm.loading = true;
        vm.pagination = {
            pageNumber: 1,
            totalPages: 10,
            pageSize: 24
        };
        vm.searchQuery = "";
        vm.installState = {
            status: "",
            progress: 0,
            type: "ok"
        };
        vm.selectCategory = selectCategory;
        vm.showPackageDetails = showPackageDetails;
        vm.setPackageViewState = setPackageViewState;
        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.goToPage = goToPage;
        vm.installPackage = installPackage;
        vm.downloadPackage = downloadPackage;
        vm.openLightbox = openLightbox;
        vm.closeLightbox = closeLightbox;
        vm.search = search;

        var currSort = "Latest";
        //used to cancel any request in progress if another one needs to take it's place
        var canceler = null;

        function getActiveCategory() {
            if (vm.searchQuery !== "") {
                return "";
            }
            for (var i = 0; i < vm.categories.length; i++) {
                if (vm.categories[i].active === true) {
                    return vm.categories[i].name;
                }
            }
            return "";
        }

        function init() {

            vm.loading = true;

            $q.all([
                    ourPackageRepositoryResource.getCategories()
                    .then(function(cats) {
                        vm.categories = cats;
                    }),
                    ourPackageRepositoryResource.getPopular(8)
                    .then(function(pack) {
                        vm.popular = pack.packages;
                    }),
                    ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize, currSort)
                    .then(function(pack) {
                        vm.packages = pack.packages;
                        vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                    })
                ])
                .then(function() {
                    vm.loading = false;
                });

        }

        function selectCategory(selectedCategory, categories) {
            var reset = false;
            for (var i = 0; i < categories.length; i++) {
                var category = categories[i];
                if (category.name === selectedCategory.name && category.active === true) {
                    //it's already selected, let's unselect to show all again
                    reset = true;
                }
                category.active = false;
            }

            vm.loading = true;
            vm.searchQuery = "";
            var searchCategory = selectedCategory.name;
            if (reset === true) {
                searchCategory = "";
            }

            currSort = "Latest";

            $q.all([
                    ourPackageRepositoryResource.getPopular(8, searchCategory)
                    .then(function(pack) {
                        vm.popular = pack.packages;
                    }),
                    ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize, currSort, searchCategory, vm.searchQuery)
                    .then(function(pack) {
                        vm.packages = pack.packages;
                        vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                        vm.pagination.pageNumber = 1;
                    })
                ])
                .then(function() {
                    vm.loading = false;
                    selectedCategory.active = reset === false;
                });
        }

        function showPackageDetails(selectedPackage) {
            ourPackageRepositoryResource.getDetails(selectedPackage.id)
                .then(function (pack) {
                    packageResource.validateInstalled(pack.name, pack.latestVersion)
                        .then(function() {
                            //ok, can install
                            vm.package = pack;
                            vm.package.isValid = true;
                            vm.packageViewState = "packageDetails";
                        }, function() {
                            //nope, cannot install
                            vm.package = pack;
                            vm.package.isValid = false;
                            vm.packageViewState = "packageDetails";
                        })
                });
        }

        function setPackageViewState(state) {
            if(state) {
                vm.packageViewState = state;
            }
        }

        function nextPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, currSort, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        function prevPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, currSort, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        function goToPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, currSort, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        function downloadPackage(selectedPackage) {
            vm.loading = true;

            packageResource
                .fetch(selectedPackage.id)
                .then(function(pack) {
                        vm.packageViewState = "packageInstall";
                        vm.loading = false;
                        vm.localPackage = pack;
                        vm.localPackage.allowed = true;
                }, function (evt, status, headers, config) {
                    
                    if (status == 400) {
                        //it's a validation error
                        vm.installState.type = "error";
                        vm.zipFile.serverErrorMessage = evt.message;
                    }
                });
        }

        function error(e, args) {
            //This will return a rejection meaning that the promise change above will stop
            return $q.reject();
        }

        function installPackage(selectedPackage) {

            vm.installState.status = localizationService.localize("packager_installStateImporting");
            vm.installState.progress = "0";

            packageResource
                .import(selectedPackage)
                .then(function(pack) {
                        vm.installState.status = localizationService.localize("packager_installStateInstalling");
                        vm.installState.progress = "33";
                        return packageResource.installFiles(pack);
                    },
                    error)
                .then(function(pack) {
                        vm.installState.status = localizationService.localize("packager_installStateRestarting");
                        vm.installState.progress = "66";
                        return packageResource.installData(pack);
                    },
                    error)
                .then(function(pack) {
                        vm.installState.status = localizationService.localize("packager_installStateComplete");
                        vm.installState.progress = "100";
                        return packageResource.cleanUp(pack);
                    },
                    error)
                .then(function(result) {

                        if (result.postInstallationPath) {
                            //Put the redirect Uri in a cookie so we can use after reloading
                            localStorageService.set("packageInstallUri", result.postInstallationPath);
                        }

                        //reload on next digest (after cookie)
                        $timeout(function() {
                            window.location.reload(true);
                        });

                    },
                    error);
        }

        function openLightbox(itemIndex, items) {
            vm.lightbox = {
                show: true,
                items: items,
                activeIndex: itemIndex
            };
        }

        function closeLightbox() {
            vm.lightbox.show = false;
            vm.lightbox = null;
        }


        var searchDebounced = _.debounce(function(e) {

            $scope.$apply(function () {

                //a canceler exists, so perform the cancelation operation and reset
                if (canceler) {
                    canceler.resolve();
                    canceler = $q.defer();
                }
                else {
                    canceler = $q.defer();
                }

                currSort = vm.searchQuery ? "Default" : "Latest";

                ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1,
                        vm.pagination.pageSize,
                        currSort,
                        "",
                        vm.searchQuery,
                        canceler)
                    .then(function(pack) {
                        vm.packages = pack.packages;
                        vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                        vm.pagination.pageNumber = 1;
                        vm.loading = false;
                        //set back to null so it can be re-created
                        canceler = null;
                    });

            });

        }, 200);

        function search(searchQuery) {
            vm.loading = true;
            searchDebounced();
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.RepoController", PackagesRepoController);

})();
