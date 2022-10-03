(function () {
    "use strict";

    function PackagesRepoController($scope, $timeout, ourPackageRepositoryResource, $q, localizationService) {

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
        vm.selectCategory = selectCategory;
        vm.showPackageDetails = showPackageDetails;
        vm.setPackageViewState = setPackageViewState;
        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.goToPage = goToPage;
        vm.openLightbox = openLightbox;
        vm.closeLightbox = closeLightbox;
        vm.search = search;
        vm.installCompleted = false;
        vm.highlightedPackageCollections = [];
        vm.labels = {};

        var defaultSort = "Latest";
        var currSort = defaultSort;

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
            localizationService.localizeMany(["packager_packagesPopular", "packager_packagesPromoted"])
                .then(function (labels) {
                    vm.labels.popularPackages = labels[0];
                    vm.labels.promotedPackages = labels[1];

                    var popularPackages, promotedPackages;
                    $q.all([
                        ourPackageRepositoryResource.getCategories()
                            .then(function (cats) {
                                vm.categories = cats.filter(function (cat) {
                                    return cat.name !== "Umbraco Pro";
                                });
                            }),
                        ourPackageRepositoryResource.getPopular(10)
                            .then(function (pack) {
                                popularPackages = { title: vm.labels.popularPackages, packages: pack.packages };
                            }),
                        ourPackageRepositoryResource.getPromoted(20)
                            .then(function (pack) {
                                promotedPackages = { title: vm.labels.promotedPackages, packages: pack.packages };
                            }),
                        ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize, currSort)
                            .then(function (pack) {
                                vm.packages = pack.packages;
                                vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                            })
                        ])
                        .then(function () {
                            vm.highlightedPackageCollections = [popularPackages, promotedPackages];
                            vm.loading = false;
                        });
                });
        }

        function selectCategory(selectedCategory, categories) {

            for (var i = 0; i < categories.length; i++) {
                var category = categories[i];
                if (category.name === selectedCategory.name) {
                    //it's already selected, let's unselect to show all again
                    if (category.active === true) {
                        category.active = false;
                    }
                    else {
                        category.active = true;
                    }
                }
                else {
                    category.active = false;
                }
            }

            vm.loading = true;
            vm.searchQuery = "";

            var reset = selectedCategory.active === false;
            var searchCategory = reset ? "" : selectedCategory.name;

            currSort = defaultSort;

            var popularPackages, promotedPackages;
            $q.all([
                ourPackageRepositoryResource.getPopular(10, searchCategory)
                    .then(function (pack) {
                        popularPackages = { title: vm.labels.popularPackages, packages: pack.packages };
                    }),
                ourPackageRepositoryResource.getPromoted(20, searchCategory)
                    .then(function (pack) {
                        promotedPackages = { title: vm.labels.promotedPackages, packages: pack.packages };
                    }),
                ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize, currSort, searchCategory, vm.searchQuery)
                    .then(function (pack) {
                        vm.packages = pack.packages;
                        vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                        vm.pagination.pageNumber = 1;
                    })
                ])
                .then(function () {
                    vm.highlightedPackageCollections = [popularPackages, promotedPackages];
                    vm.loading = false;
                });
        }

        function showPackageDetails(selectedPackage) {
            ourPackageRepositoryResource.getDetails(selectedPackage.id)
                .then(function (pack) {
                    vm.package = pack;
                    vm.packageViewState = "packageDetails";
                });
        }

        function setPackageViewState(state) {
            if (state) {
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
        var previousElement = null;

        function openLightbox(itemIndex, items) {

            previousElement = ( document.activeElement || document.body );

            vm.lightbox = {
                show: true,
                items: items,
                activeIndex: itemIndex,
                focus: true
            };
        }

        function closeLightbox() {
            vm.lightbox.show = false;
            vm.lightbox = null;

            if(previousElement){

                setTimeout(function(){ 
                    previousElement.focus();
                    previousElement = null;
                  }, 100) 
            }
        }


        var searchDebounced = _.debounce(function (e) {

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
                    .then(function (pack) {
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

        vm.reloadPage = function () {
            //reload on next digest (after cookie)
            $timeout(function () {
                window.location.reload(true);
            });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.RepoController", PackagesRepoController);

})();
