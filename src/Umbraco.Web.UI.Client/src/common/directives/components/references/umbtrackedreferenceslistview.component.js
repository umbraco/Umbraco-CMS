(function () {
    'use strict';

    /**
     * A component to render the items from a selection which are used in a relation
     */

    function umbTrackedReferencesListViewController($q, trackedReferencesResource, localizationService)
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
        vm.hasContentReferencesInListView = false;

        vm.changeMediaPageNumber = changeMediaPageNumber;
        vm.mediaOptions = {};
        vm.mediaOptions.entityType = "MEDIA";
        // TODO: rename this prop. it has nothing to do with list-views. suggestion, rename to: hasReferencesDescendants
        vm.hasMediaReferencesInListView = false;

        vm.$onInit = onInit;

        function onInit() {

            this.loading = true;
            this.hideNoResult = this.hideNoResult || false;

            $q.all([checkContentListViewUsage(), checkMediaListViewUsage()]).then(function () {
                vm.loading = false;
                if(vm.onLoadingComplete) {
                    vm.onLoadingComplete();
                }
            });
        }

        function changeContentPageNumber(pageNumber) {
            vm.contentOptions.pageNumber = pageNumber;
            checkContentListViewUsage();
        }

        function changeMediaPageNumber(pageNumber) {
            vm.mediaOptions.pageNumber = pageNumber;
            checkMediaListViewUsage();
        }

        function checkContentListViewUsage() {
             var ids = vm.selection.map(s => s.id);

             return trackedReferencesResource.checkLinkedItems(ids, vm.contentOptions)
                  .then(function (data) {
                      vm.referencedContentItems = data;

                      if (data.items.length > 0) {
                          vm.hasContentReferencesInListView = data.items.length > 0;
                          activateWarning();
                      }
                  });
        }

        function checkMediaListViewUsage() {
            var ids = vm.selection.map(s => s.id);

            return trackedReferencesResource.checkLinkedItems(ids, vm.mediaOptions)
                  .then(function (data) {
                      vm.referencedMediaItems = data;

                      if (data.items.length > 0) {
                          vm.hasMediaReferencesInListView = data.items.length > 0;
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

    var umbTrackedReferencesListViewComponent = {
        templateUrl: 'views/components/references/umb-tracked-references-listview.html',
        transclude: true,
        bindings: {
            selection: "<",
            hideNoResult: "<?",
            onWarning: "&?",
            onLoadingComplete: "&?"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesListViewController
    };

    angular.module('umbraco.directives').component('umbTrackedReferencesListview', umbTrackedReferencesListViewComponent);

})();
