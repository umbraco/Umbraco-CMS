(function () {
    'use strict';

    /**
     * A component to render a table for the items that have tracked references
     */

    function umbTrackedReferencesListViewTableController(editorService, overlayService, udiParser)
    {
        var vm = this;

        vm.changePageNumber = changePageNumber;
        vm.openItem = openItem;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
        }

        function openItem(item) {
            var editorModel = {
                id: item.id,
                submit: function (model) {
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };

            overlayService.close();

            // extract the entity type from the udi
            var udi = udiParser.parse(item.udi);

            if (udi && udi.entityType === "document")
            {
                editorService.contentEditor(editorModel);
                return;
            }

            if (udi && udi.entityType === "media")
            {
                editorService.mediaEditor(editorModel);
                return;
            }

            if (udi && udi.entityType === "member")
            {
                editorModel.id = item.key;
                editorService.memberEditor(editorModel);
                return;
            }
        }
    }

    var umbTrackedReferencesListViewTableComponent = {
        templateUrl: 'views/components/references/umb-tracked-references-listview-table.html',
        transclude: true,
        bindings: {
            pageNumber: "<",
            totalPages: "<",
            title: "<",
            items: "<",
            onPageChanged: "&"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesListViewTableController
    };

    angular.module('umbraco.directives').component('umbTrackedReferencesListviewTable', umbTrackedReferencesListViewTableComponent);

})();
