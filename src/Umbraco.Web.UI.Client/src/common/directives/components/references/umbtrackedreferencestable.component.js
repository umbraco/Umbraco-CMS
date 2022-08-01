(function () {
    'use strict';

    /**
     * A component to render a table for the tracked references of an item
     */

    function umbTrackedReferencesTableController(editorService, udiParser, navigationService, overlayService)
    {
        const vm = this;

        vm.changePageNumber = changePageNumber;
        vm.getUrl = getUrl;
        vm.referenceAnchorClicked = referenceAnchorClicked;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
        }

        function getUrl(itemUdi, itemId) {
            // extract the entity type from the udi
            const udi = udiParser.parse(itemUdi);

            if (udi) {
                let section = udi.entityType;
                let tree = udi.entityType;

                if (udi.entityType === "document") {
                    section = "content";
                    tree = "content";
                }
                else if (udi.entityType === "document-blueprint") {
                    section = "settings";
                    tree = "contentBlueprints";
                }
                
                return `#/${section}/${tree}/edit/${itemId}`;
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

            let editorModel = {
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
            const udi = udiParser.parse(reference.udi);

            if (udi)
            {
                if (udi.entityType === "document-blueprint") {
                    editorService.contentBlueprintEditor(editorModel);
                    return;
                }

                if (udi.entityType === "document") {
                    editorService.contentEditor(editorModel);
                    return;
                }

                if (udi.entityType === "media") {
                    editorService.mediaEditor(editorModel);
                    return;
                }

                if (udi.entityType === "member") {
                    editorModel.id = reference.key;
                    editorService.memberEditor(editorModel);
                    return;
                }
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
