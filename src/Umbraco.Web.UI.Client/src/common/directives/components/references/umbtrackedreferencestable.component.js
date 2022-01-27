(function () {
    'use strict';

    /**
     * A component to render a table for the tracked references of an item
     */

    function umbTrackedReferencesTableController()
    {
        var vm = this;

        vm.changePageNumber = changePageNumber;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
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
