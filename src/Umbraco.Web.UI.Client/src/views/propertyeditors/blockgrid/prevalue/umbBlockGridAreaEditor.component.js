(function () {
    "use strict";

    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridAreaEditor
     * @function
     *
     * @description
     * The component for the block grid area prevalue editor.
     */
    angular
        .module("umbraco")
        .component("umbBlockGridAreaEditor", {
            templateUrl: "views/propertyeditors/blockgrid/prevalue/umb-block-grid-area-editor.html",
            controller: BlockGridAreaController,
            controllerAs: "vm",
            bindings: {
                model: "=",
                block: "<"
            },
            require: {
                propertyForm: "^form"
            }
        });

    function BlockGridAreaController($scope, $element, assetsService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;
        vm.blockGridColumns = 0;

        vm.$onInit = function() {
            console.log("BlockGridAreaController")

            // TODO: Watch for vm.block.areaGridColumns
            // TODO: watch for column span options as fallback for areaGridColumns.

            vm.blockGridColumns = vm.block.areaGridColumns || 12;


            assetsService.loadJs('lib/sortablejs/Sortable.min.js', $scope).then(onLoaded);
        };

        function onLoaded() {
            vm.loading = false;
            initializeSortable();
        }

        function initializeSortable() {

            const gridLayoutContainerEl = $element[0].querySelector('.umb-block-grid-area-editor__grid-wrapper');

            console.log("gridLayoutContainerEl", gridLayoutContainerEl);

            const sortable = Sortable.create(gridLayoutContainerEl, {
                sort: true,  // sorting inside list
                animation: 150,  // ms, animation speed moving items when sorting, `0` — without animation
                easing: "cubic-bezier(1, 0, 0, 1)", // Easing for animation. Defaults to null. See https://easings.net/ for examples.
                cancel: '',
                draggable: ".umb-block-grid-area-editor__area",  // Specifies which items inside the element should be draggable
                ghostClass: "umb-block-grid-area-editor__area-placeholder"
            });

        }

        vm.onAreaClick = function($event, area) {
            console.log(area);
        }

        vm.onNewAreaClick = function() {
            vm.model.push({
                'key': String.CreateGuid(),
                'alias': '',
                'columnSpan': (vm.blockGridColumns),
                'rowSpan': 1
            })
        }
        
        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        }
        
        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });
        
    }

})();
