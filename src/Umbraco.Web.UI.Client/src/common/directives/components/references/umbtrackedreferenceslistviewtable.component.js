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
        //vm.getLanguageLabel = getLanguageLabel;

        function changePageNumber(pageNumber) {
            vm.onPageChanged({ 'pageNumber' : pageNumber });
        }

        function openItem(item) {
            var editorModel = {
                id: item.id,
                //culture: item.culture,
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

        //function getLanguageLabel(culture) {
        //    if (vm.languages.length > 0) {

        //        var lang = _.find(vm.languages,
        //            function (l) {
        //                return l.culture.toLowerCase() === culture.toLowerCase();
        //            });

        //        if (lang) {
        //            return lang.name;
        //        }
        //    }

        //    return culture;
        //}


        //function init() { 
        //    languageResource.getAll().then(function (data) {
        //        vm.languages = data;
        //        //vm.totalPages = Math.ceil(vm.relations.length / vm.pageSize);
        //    });
        //}

        //init();
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
