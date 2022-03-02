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
      
        vm.changeContentPageNumber = changeContentPageNumber;
        vm.contentOptions = {};
        vm.contentOptions.entityType = "DOCUMENT";
        // TODO: rename this prop. it has nothing to do with list-views. suggestion, rename to: hasReferencesDescendants
        vm.hasContentReferences = false;

        vm.changeMediaPageNumber = changeMediaPageNumber;
        vm.mediaOptions = {};
        vm.mediaOptions.entityType = "MEDIA";
        // TODO: rename this prop. it has nothing to do with list-views. suggestion, rename to: hasReferencesDescendants
        vm.hasMediaReferences = false;

        vm.$onInit = onInit;

        function onInit() {

            this.loading = true;
            this.hideNoResult = this.hideNoResult || false;

            $q.all([checkContentBulkActionUsage(), checkMediaBulkActionUsage()]).then(function () {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
            });
        }

        function changeContentPageNumber(pageNumber) {
            vm.contentOptions.pageNumber = pageNumber;
            checkContentBulkActionUsage();
        }

        function changeMediaPageNumber(pageNumber) {
            vm.mediaOptions.pageNumber = pageNumber;
            checkMediaBulkActionUsage();
        }

        function checkContentBulkActionUsage() {
             var ids = vm.selection.map(s => s.id);

             return trackedReferencesResource.checkLinkedItems(ids, vm.contentOptions)
                  .then(function (data) {
                      vm.referencedContentItems = data;

                      if (data.items.length > 0) {
                          vm.hasContentReferences = data.items.length > 0;
                          activateWarning();
                      }
                  });
        }

        function checkMediaBulkActionUsage() {
            var ids = vm.selection.map(s => s.id);

            return trackedReferencesResource.checkLinkedItems(ids, vm.mediaOptions)
                  .then(function (data) {
                      vm.referencedMediaItems = data;

                      if (data.items.length > 0) {
                          vm.hasMediaReferences = data.items.length > 0;
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
