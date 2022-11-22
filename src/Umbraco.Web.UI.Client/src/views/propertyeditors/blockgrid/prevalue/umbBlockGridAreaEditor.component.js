(function () {
    "use strict";


    // Utils:

    function getInterpolatedIndexOfPositionInWeightMap(target, weights) {
        const map = [0];
        weights.reduce((a, b, i) => { return map[i+1] = a+b; }, 0);
        const foundValue = map.reduce((a, b) => {
            let aDiff = Math.abs(a - target);
            let bDiff = Math.abs(b - target);
    
            if (aDiff === bDiff) {
                return a < b ? a : b;
            } else {
                return bDiff < aDiff ? b : a;
            }
        })
        const foundIndex = map.indexOf(foundValue);
        const targetDiff = (target-foundValue);
        let interpolatedIndex = foundIndex;
        if (targetDiff < 0 && foundIndex === 0) {
            // Don't adjust.
        } else if (targetDiff > 0 && foundIndex === map.length-1) {
            // Don't adjust.
        } else {
            const foundInterpolationWeight = weights[targetDiff >= 0 ? foundIndex : foundIndex-1];
            interpolatedIndex += foundInterpolationWeight === 0 ? interpolatedIndex : (targetDiff/foundInterpolationWeight)
        }
        return interpolatedIndex;
    }

    function getAccumulatedValueOfIndex(index, weights) {
        let i = 0, len = Math.min(index, weights.length), calc = 0;
        while(i<len) {
            calc += weights[i++];
        }
        return calc;
    }

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
                model: "<",
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

    function BlockGridAreaController($scope, localizationService, editorService, overlayService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;
        vm.rootLayoutColumns = 12;

        vm.$onInit = function() {

            vm.rootLayoutColumns = vm.gridColumns;
            initializeSortable();
            vm.loading = false;

        };

        function initializeSortable() {

            vm.sorterOptions = {
                resolveVerticalDirection: resolveVerticalDirection,
                compareElementToModel: (el, modelEntry) => modelEntry.key === el.dataset.areaKey,
                querySelectModelToElement: (container, modelEntry) => container.querySelector(`[data-area-key='${modelEntry.key}']`),
                itemHasNestedContainersResolver: () => false,// We never have nested in this case.
                containerSelector: ".umb-block-grid-area-editor__grid-wrapper",
                itemSelector: ".umb-block-grid-area-editor__area",
                placeholderClass: "umb-block-grid-area-editor__area-placeholder",
                onSync: onSortSync
            }

            function onSortSync() {
                $scope.$evalAsync();
                setDirty();
            }

            function resolveVerticalDirection(data) {

                /** We need some data about the grid to figure out if there is room to be placed next to the found element */
                const approvedContainerComputedStyles = getComputedStyle(data.containerElement);
                const gridColumnGap = Number(approvedContainerComputedStyles.columnGap.split("px")[0]) || 0;
                const gridColumnNumber = vm.rootLayoutColumns;
    
                const foundElColumns = parseInt(data.relatedElement.dataset.colSpan, 10);
                const currentElementColumns = data.item.columnSpan;

                if(currentElementColumns >= gridColumnNumber) {
                    return true;
                }
    
                // Get grid template:
                const approvedContainerGridColumns = approvedContainerComputedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x)).filter(n => n > 0).map((n, i, list) => list.length === i ? n : n + gridColumnGap);
    
                // ensure all columns are there.
                // This will also ensure handling non-css-grid mode,
                // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
                let amountOfColumnsInWeightMap = approvedContainerGridColumns.length;
                const amountOfUnknownColumns = gridColumnNumber-amountOfColumnsInWeightMap;
                if(amountOfUnknownColumns > 0) {
                    let accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, approvedContainerGridColumns) || 0;
                    const layoutWidth = data.containerRect.width;
                    const missingColumnWidth = (layoutWidth-accumulatedValue)/amountOfUnknownColumns;
                    if(missingColumnWidth > 0) {
                        while(amountOfColumnsInWeightMap++ < gridColumnNumber) {
                            approvedContainerGridColumns.push(missingColumnWidth);
                        }
                    }
                }
    
                let offsetPlacement = 0;
                /* If placeholder is in this same line, we want to assume that it will offset the placement of the found element, 
                which provides more potential space for the item to drop at.
                This is relevant in this calculation where we look at the space to determine if its a vertical or horizontal drop in relation to the found element.
                */
                if(data.placeholderIsInThisRow && data.elementRect.left < data.relatedRect.left) {
                    offsetPlacement = -(data.elementRect.width + gridColumnGap);
                }
    
                const relatedStartX = Math.max(data.relatedRect.left - data.containerRect.left + offsetPlacement, 0);
                const relatedStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(relatedStartX, approvedContainerGridColumns));
    
                // If the found related element does not have enough room after which for the current element, then we go vertical mode:
                return (relatedStartCol + (data.horizontalPlaceAfter ? foundElColumns : 0) + currentElementColumns > gridColumnNumber);
                
            }
    

        }

        vm.editArea = function(area) {
            vm.openAreaOverlay(area);
        }

        vm.requestDeleteArea = function (area) {
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
