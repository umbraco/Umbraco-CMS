(function () {
    'use strict';

    /**
     * A component to render the items from a selection which are used in a relation
     */

    function umbTrackedReferencesBulkActionController($q, trackedReferencesResource, localizationService)
    {
        var vm = this;

        vm.referencedItemsTitle = "These items are referenced";

        localizationService.localize("references_labelUsedItems").then(function (value) {
            vm.referencedItemsTitle = value;
        });
      
        vm.changeReferencesPageNumber = changeReferencesPageNumber;
        vm.referencesOptions = {};
        vm.referencesOptions.filterMustBeIsDependency = true;
        vm.hasReferences = false;

        vm.$onInit = onInit;

        function onInit() {

            this.loading = true;
            this.hideNoResult = this.hideNoResult || false;

            loadContentBulkActionUsage().then(function () {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
            });
        }

        function changeReferencesPageNumber(pageNumber) {
            vm.referencesOptions.pageNumber = pageNumber;
            loadContentBulkActionUsage();
        }

        function loadContentBulkActionUsage() {
             var ids = vm.selection.map(s => s.id);

             return trackedReferencesResource.getPagedReferencedItems(ids, vm.referencesOptions)
                  .then(function (data) {
                      vm.referencedItems = data;

                      if (data.items.length > 0) {
                          vm.hasReferences = data.items.length > 0;
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

    var umbTrackedReferencesBulkActionComponent = {
        templateUrl: 'views/components/references/umb-tracked-references-bulk-action.html',
        transclude: true,
        bindings: {
            selection: "<",
            hideNoResult: "<?",
            onWarning: "&?",
            onLoadingComplete: "&?"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesBulkActionController
    };

    angular.module('umbraco.directives').component('umbTrackedReferencesBulkAction', umbTrackedReferencesBulkActionComponent);

})();
