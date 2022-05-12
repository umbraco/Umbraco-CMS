(function () {
    'use strict';

    /**
     * A component to render the tracked references of an item
     */

    function umbTrackedReferencesController($q, trackedReferencesResource, localizationService) {

        var vm = this;

        vm.changeReferencesPageNumber = changeReferencesPageNumber;
        vm.changeDescendantsPageNumber = changeDescendantsPageNumber;

        vm.$onInit = onInit;

        function onInit() {

            vm.referencesTitle = this.hideNoneDependencies ? "The following items depend on this" : "Referenced by the following items";
            vm.referencedDescendantsTitle = this.hideNoneDependencies ? "The following descending items have dependencies" : "The following descendant items have dependencies";
            localizationService.localize(this.hideNoneDependencies ? "references_labelDependsOnThis" : "references_labelUsedByItems").then(function (value) {
                vm.referencesTitle = value;
            });

            localizationService.localize(this.hideNoneDependencies ? "references_labelDependentDescendants" : "references_labelUsedDescendants").then(function (value) {
                vm.referencedDescendantsTitle = value;
            });

            vm.descendantsOptions = {};
            vm.descendantsOptions.filterMustBeIsDependency = this.hideNoneDependencies;
            vm.hasReferencesInDescendants = false;

            vm.referencesOptions = {};
            vm.referencesOptions.filterMustBeIsDependency = this.hideNoneDependencies;
            vm.hasReferences = false;

            this.loading = true;
            this.hideNoResult = this.hideNoResult || false;

            // when vm.id == 0 it means that this is a new item, so it has no references yet
            if (vm.id === 0) {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
                return;
            }

            // Make array of promises to load:
            var promises = [loadReferencesRelations()];

            // only load descendants if we want to show them.
            if (vm.showDescendants) {
                promises.push(loadDescendantsUsage());
            }

            $q.all(promises).then(function () {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
            });
        }

        function changeReferencesPageNumber(pageNumber) {
            vm.referencesOptions.pageNumber = pageNumber;
            loadReferencesRelations();
        }

        function changeDescendantsPageNumber(pageNumber) {
            vm.descendantsOptions.pageNumber = pageNumber;
            loadDescendantsUsage();
        }

        function loadReferencesRelations() {
            return trackedReferencesResource.getPagedReferences(vm.id, vm.referencesOptions)
                .then(function (data) {
                    vm.references = data;

                    if (data.items.length > 0) {
                        vm.hasReferences = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function loadDescendantsUsage() {
            return trackedReferencesResource.getPagedDescendantsInReferences(vm.id, vm.descendantsOptions)
                .then(function (data) {
                    vm.referencedDescendants = data;

                    if (data.items.length > 0) {
                        vm.hasReferencesInDescendants = data.items.length > 0;
                        activateWarning();
                    }
                });
        }

        function activateWarning() {
            if (vm.onWarning) {
                vm.onWarning();
            }
        }
    }

    var umbTrackedReferencesComponent = {
        templateUrl: 'views/components/references/umb-tracked-references.html',
        transclude: true,
        bindings: {
            id: "<",
            hideNoResult: "<?",
            onWarning: "&?",
            onLoadingComplete: "&?",
            compact: "<?",
            showDescendants: "<?",
            hideNoneDependencies: "<?"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesController
    };

    angular.module('umbraco.directives').component('umbTrackedReferences', umbTrackedReferencesComponent);

})();
