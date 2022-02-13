(function () {
    'use strict';

    /**
     * A component to render a table for the tracked references of an item
     */

    function umbTrackedReferencesTableController(udiParser, navigationService, overlayService)
    {
        var vm = this;

        vm.changePageNumber = changePageNumber;
        vm.getUrl = getUrl;
        vm.close = close;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
        }

        function getUrl(itemUdi, itemId) {
            // extract the entity type from the udi
            var udi = udiParser.parse(itemUdi);

            if (udi) {
                var entityType = udi.entityType;

                if(udi.entityType === "document") {
                    entityType = "content";
                }
                return "#/" + entityType + "/" + entityType + "/edit/" + itemId;
            }
	          return "#";
	      }

        function close() {
            navigationService.hideMenu(); // close menu
            overlayService.close(); // close overlay
        }
    }

    var umbTrackedReferencesTableComponent = {
        templateUrl: 'views/components/references/umb-tracked-references-table.html',
        transclude: true,
        bindings: {
            pageNumber: "<",
            totalPages: "<",
            title: "<",
            items: "<",
            onPageChanged: "&"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesTableController
    };

    angular.module('umbraco.directives').component('umbTrackedReferencesTable', umbTrackedReferencesTableComponent);

})();
