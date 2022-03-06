(function () {
    'use strict';

    /**
     * A component to render a table for the tracked references of an item
     */

    function umbTrackedReferencesTableController(editorService, udiParser, navigationService, overlayService)
    {
        var vm = this;

        vm.changePageNumber = changePageNumber;
        vm.getUrl = getUrl;
        vm.referenceAnchorClicked = referenceAnchorClicked;

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

        function referenceAnchorClicked($event, reference) {


            if ($event.shiftKey || $event.ctrlKey || $event.metaKey) {
                // we will let the browser take over here.
                return;
            }

            $event.preventDefault();
            close();

            var editorModel = {
                id: reference.id,
                submit: function (model) {
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };

            overlayService.close();

            // extract the entity type from the udi
            var udi = udiParser.parse(reference.udi);

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
                editorModel.id = reference.key;
                editorService.memberEditor(editorModel);
                return;
            }
        }
    }

    var umbTrackedReferencesTableComponent = {
        templateUrl: 'views/components/references/umb-tracked-references-table.html',
        transclude: true,
        bindings: {
            pageNumber: "<",
            totalPages: "<",
            headline: "<",
            items: "<",
            showType: "<?",
            showTypeName: "<?",
            showRelationTypeName: "<?",
            onPageChanged: "&"
        },
        controllerAs: 'vm',
        controller: umbTrackedReferencesTableController
    };

    angular.module('umbraco.directives').component('umbTrackedReferencesTable', umbTrackedReferencesTableComponent);

})();
