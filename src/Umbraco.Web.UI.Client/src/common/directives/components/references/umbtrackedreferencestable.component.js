
(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTrackedReferencesTable', {
            transclude: true,
            templateUrl: 'views/components/references/umb-tracked-references-table.html',
            controller: UmbTrackedReferencesTableController,
            controllerAs: 'vm',
            bindings: {
                pageNumber: "<",
                totalPages : "<",
                title: "<",
                items : "<",
                onPageChanged: "&"
            }
        });

    function UmbTrackedReferencesTableController()
    {
        var vm = this;

        vm.changePageNumber = changePageNumber;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
        }
    }

})();
