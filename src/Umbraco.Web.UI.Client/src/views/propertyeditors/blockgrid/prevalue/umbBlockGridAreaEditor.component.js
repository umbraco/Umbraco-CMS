(function () {
    "use strict";

    function TransferProperties(fromObject, toObject) {
        for (var p in fromObject) {
            toObject[p] = fromObject[p];
        }
    }

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
                block: "<",
                allBlockTypes: "<",
                allBlockGroups: "<",
                loadedElementTypes: "<",
                gridColumns: "<"
            },
            require: {
                propertyForm: "^form"
            }
        });

    function BlockGridAreaController($scope, $element, assetsService, localizationService, editorService, overlayService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;
        vm.rootLayoutColumns = 12;

        vm.$onInit = function() {

            vm.rootLayoutColumns = vm.gridColumns;

            assetsService.loadJs('lib/sortablejs/Sortable.min.js', $scope).then(onLoaded);
        };

        function onLoaded() {
            vm.loading = false;
            initializeSortable();
        }

        function initializeSortable() {

            function _sync(evt) {

                const oldIndex = evt.oldIndex,
                      newIndex = evt.newIndex;

                vm.model.splice(newIndex, 0, vm.model.splice(oldIndex, 1)[0]);
                
            }

            const gridContainerEl = $element[0].querySelector('.umb-block-grid-area-editor__grid-wrapper');

            const sortable = Sortable.create(gridContainerEl, {
                sort: true,  // sorting inside list
                animation: 150,  // ms, animation speed moving items when sorting, `0` — without animation
                easing: "cubic-bezier(1, 0, 0, 1)", // Easing for animation. Defaults to null. See https://easings.net/ for examples.
                cancel: '',
                draggable: ".umb-block-grid-area-editor__area",  // Specifies which items inside the element should be draggable
                ghostClass: "umb-block-grid-area-editor__area-placeholder",
                onAdd: function (evt) {
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onUpdate: function (evt) {
                    _sync(evt);
                    $scope.$evalAsync();
                }
            });

            // TODO: setDirty if sort has happend.

        }

        vm.editArea = function(area) {
            vm.openAreaOverlay(area);
        }

        vm.requestDeleteArea = function (area) {
            // TODO: Translations
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockAreaMessage", "blockEditor_confirmDeleteBlockAreaNotice"]).then(function (data) {
                overlayService.confirmDelete({
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [area.alias]),
                    confirmMessage: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.deleteArea(area);
                        overlayService.close();
                    }
                });
            });
        }
        vm.deleteArea = function(area) {
            const index = vm.model.findIndex(x => x.key === area.key);
            if(index !== -1) {
                vm.model.splice(index, 1);
            }
            setDirty();
        }

        vm.onNewAreaClick = function() {

            const areaGridColumns = (vm.block.areaGridColumns || vm.rootLayoutColumns)
            const columnSpan = areaGridColumns/2 === Math.round(areaGridColumns/2) ? areaGridColumns/2 : areaGridColumns;

            const newArea = {
                'key': String.CreateGuid(),
                'alias': '',
                'columnSpan': columnSpan,
                'rowSpan': 1,
                'minAllowed': 0,
                'maxAllowed': null,
                'specifiedAllowance': [
                    /*{
                        'elementTypeKey': 345,
                        'min': 0,
                        'max': null
                    }*/
                ]

            };
            vm.model.push(newArea)
            vm.openAreaOverlay(newArea);
            setDirty();
        }

        vm.openArea = null;
        vm.openAreaOverlay = function (area) {

            // TODO: use the right localization key:
            localizationService.localize("blockEditor_blockConfigurationOverlayTitle").then(function (localized) {

                var clonedAreaData = Utilities.copy(area);
                vm.openArea = area;

                function updateTitle() {
                    overlayModel.title = localizationService.tokenReplace(localized, [clonedAreaData.alias]);
                }

                const areaIndex = vm.model.indexOf(area);
                const otherAreas = [...vm.model];
                otherAreas.splice(areaIndex, 1);

                var overlayModel = {
                    otherAreaAliases: otherAreas.map(x => x.alias),
                    area: clonedAreaData,
                    updateTitle: updateTitle,
                    allBlockTypes: vm.allBlockTypes,
                    allBlockGroups: vm.allBlockGroups,
                    loadedElementTypes: vm.loadedElementTypes,
                    view: "views/propertyeditors/blockgrid/prevalue/blockgrid.blockconfiguration.area.overlay.html",
                    size: "small",
                    submit: function(overlayModel) {
                        TransferProperties(overlayModel.area, area);
                        overlayModel.close();
                        setDirty();
                    },
                    close: function() {
                        editorService.close();
                        vm.openArea = null;
                    }
                };

                updateTitle();

                // open property settings editor
                editorService.open(overlayModel);

            });

        };
        
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
