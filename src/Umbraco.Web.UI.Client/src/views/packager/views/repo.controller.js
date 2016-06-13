(function () {
    "use strict";

    function PackagesRepoController($scope, $route, $location, $timeout, ourPackageRepositoryResource, $q) {

        var vm = this;

        vm.packageViewState = "packageList";
        vm.categories = [];
        vm.loading = false;
        vm.pagination = {
            pageNumber: 1,
            totalPages: 10,
            pageSize: 2
        };
        vm.searchQuery = "";

        vm.selectCategory = selectCategory;
        vm.showPackageDetails = showPackageDetails;
        vm.setPackageViewState = setPackageViewState;
        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.goToPage = goToPage;

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
                    ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize)
                    .then(function(pack) {
                        vm.packages = pack.packages;
                        vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                    })
                ])
                .then(function() {
                    vm.loading = false;
                });

            $scope.$watch(function() {
                return vm.searchQuery;
            }, _.debounce(function (newVal, oldVal) {
                $scope.$apply(function () {
                    if (vm.searchQuery) {
                        if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                            vm.loading = true;
                            
                            //a canceler exists, so perform the cancelation operation and reset
                            if (canceler) {
                                canceler.resolve();
                                canceler = $q.defer();
                            }
                            else {
                                canceler = $q.defer();
                            }

                            ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1,
                                    vm.pagination.pageSize,
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
                        }
                    }
                    else {
                        vm.loading = false;
                    }
                });
            }, 200));

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

            $q.all([                    
                    ourPackageRepositoryResource.getPopular(8, searchCategory)
                    .then(function(pack) {
                        vm.popular = pack.packages;
                    }),
                    ourPackageRepositoryResource.search(vm.pagination.pageNumber - 1, vm.pagination.pageSize, searchCategory, vm.searchQuery)
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
                .then(function(pack) {
                    vm.package = pack;
                    vm.packageViewState = "packageDetails";
                });

        }

        function setPackageViewState(state) {
            if(state) {
                vm.packageViewState = state;
            }
        }

        function nextPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        function prevPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        function goToPage(pageNumber) {
            ourPackageRepositoryResource.search(pageNumber - 1, vm.pagination.pageSize, getActiveCategory(), vm.searchQuery)
                .then(function (pack) {
                    vm.packages = pack.packages;
                    vm.pagination.totalPages = Math.ceil(pack.total / vm.pagination.pageSize);
                });
        }

        init();

    }

    angular.module("umbraco").controller("Umbraco.Editors.Packages.RepoController", PackagesRepoController);

})();
