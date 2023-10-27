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



    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridEntries
     * @description
     * renders all blocks for a given list for the block grid editor
     */

    angular
        .module("umbraco")
        .component("umbBlockGridEntries", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-entries.html',
            controller: BlockGridEntriesController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                entries: "<",
                layoutColumns: "<",
                areaKey: "<?",
                parentBlock: "<?",
                parentForm: "<?",
                propertyEditorForm: "<?",
                depth: "@",
                createLabel: "@?"
            }
        }
    );

    function BlockGridEntriesController($element, $scope) {

        const unsubscribe = [];
        const vm = this;
        vm.invalidAmount = false;
        vm.areaConfig = null;
        vm.locallyAvailableBlockTypes = 0;
        vm.invalidBlockTypes = [];

        vm.showNotAllowedUI = false;
        let currentContainedPropertyEditorProxies = [];

        vm.$onInit = function () {

            initializeSorter();

            if(vm.parentBlock) {
                vm.areaConfig = vm.parentBlock.config.areas.find(area => area.key === vm.areaKey);
            }

            vm.locallyAvailableBlockTypes = vm.blockEditorApi.internal.getAllowedTypesOf(vm.parentBlock, vm.areaKey);

            unsubscribe.push($scope.$watch('vm.entries', onLocalAmountOfBlocksChanged, true));
        };

        function onLocalAmountOfBlocksChanged() {

            if (vm.entriesForm && vm.areaConfig) {

                var isMinRequirementGood = vm.entries.length >= vm.areaConfig.minAllowed;
                vm.entriesForm.areaMinCount.$setValidity("areaMinCount", isMinRequirementGood);

                var isMaxRequirementGood = vm.areaConfig.maxAllowed == null || vm.entries.length <= vm.areaConfig.maxAllowed;
                vm.entriesForm.areaMaxCount.$setValidity("areaMaxCount", isMaxRequirementGood);

                vm.invalidBlockTypes = [];

                vm.areaConfig.specifiedAllowance.forEach(allowance => {

                    const minAllowed = allowance.minAllowed || 0;
                    const maxAllowed = allowance.maxAllowed || 0;

                    // For block groups:
                    if(allowance.groupKey) {

                        const groupElementTypeKeys = vm.locallyAvailableBlockTypes.filter(blockType => blockType.blockConfigModel.groupKey === allowance.groupKey && blockType.blockConfigModel.allowInAreas === true).map(x => x.blockConfigModel.contentElementTypeKey);
                        const groupAmount = vm.entries.filter(entry => groupElementTypeKeys.indexOf(entry.$block.data.contentTypeKey) !== -1).length;

                        if(groupAmount < minAllowed || (maxAllowed > 0 && groupAmount > maxAllowed)) {
                            vm.invalidBlockTypes.push({
                                'groupKey': allowance.groupKey,
                                'name': vm.blockEditorApi.internal.getBlockGroupName(allowance.groupKey),
                                'amount': groupAmount,
                                'minRequirement': minAllowed,
                                'maxRequirement': maxAllowed
                            });
                        }
                    } else
                    // For specific elementTypes:
                    if(allowance.elementTypeKey) {

                        const amount = vm.entries.filter(entry => entry.$block.data.contentTypeKey === allowance.elementTypeKey).length;

                        if(amount < minAllowed || (maxAllowed > 0 && amount > maxAllowed)) {
                            vm.invalidBlockTypes.push({
                                'key': allowance.elementTypeKey,
                                'name': vm.locallyAvailableBlockTypes.find(blockType => blockType.blockConfigModel.contentElementTypeKey === allowance.elementTypeKey).elementTypeModel.name,
                                'amount': amount,
                                'minRequirement': minAllowed,
                                'maxRequirement': maxAllowed
                            });
                        }
                    }
                });
                var isTypeRequirementGood = vm.invalidBlockTypes.length === 0;
                vm.entriesForm.areaTypeRequirements.$setValidity("areaTypeRequirements", isTypeRequirementGood);


                vm.invalidAmount = !isMinRequirementGood || !isMaxRequirementGood || !isTypeRequirementGood;

                $element.toggleClass("--invalid", vm.invalidAmount);
            }
        }

        // Used by umb block grid entries component, to trigger other blocks to update.
        vm.notifyVisualUpdate = notifyVisualUpdate;
        function notifyVisualUpdate() {
            $scope.$broadcast("blockGridEditorVisualUpdate", {areaKey: vm.areaKey});
        }

        function removeAllContainedPropertyEditorProxies() {
            currentContainedPropertyEditorProxies.forEach(slotName => {
                removePropertyEditorProxies(slotName);
            });
        }
        function removePropertyEditorProxies(slotName) {
            const event = new CustomEvent("UmbBlockGrid_RemoveProperty", {composed: true, bubbles: true, detail: {'slotName': slotName}});
            $element[0].dispatchEvent(event);
        }




        function resolveVerticalDirection(data) {

            /** We need some data about the grid to figure out if there is room to be placed next to the found element */
            const approvedContainerComputedStyles = getComputedStyle(data.containerElement);
            const gridColumnGap = Number(approvedContainerComputedStyles.columnGap.split("px")[0]) || 0;
            const gridColumnNumber = parseInt(approvedContainerComputedStyles.getPropertyValue("--umb-block-grid--grid-columns"), 10);

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
                        const missingColumnWidth = (layoutWidth-accumulatedValue)/amountOfUnknownColumns;if(missingColumnWidth > 0) {
                        while(amountOfColumnsInWeightMap++ < gridColumnNumber) {
                            approvedContainerGridColumns.push(missingColumnWidth);
                        }
                    }}

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




        function initializeSorter() {
            vm.sorterOptions = {
                ownerVM: vm,
                resolveVerticalDirection: resolveVerticalDirection,
                dataTransferResolver: (dataTransfer, item) => {dataTransfer.setData("text/plain", item.$block.label)}, // (Optional) Append OS data to the moved item.
                compareElementToModel: (el, modelEntry) => modelEntry.contentUdi === el.dataset.elementUdi,
                querySelectModelToElement: (container, modelEntry) => container.querySelector(`[data-element-udi='${modelEntry.contentUdi}']`),
                itemHasNestedContainersResolver: (foundEl) => foundEl.classList.contains('--has-areas'), // (Optional) improve performance for recognizing if an items has inner containers.
                identifier: "BlockGridEditor_"+vm.blockEditorApi.internal.uniqueEditorKey,
                boundarySelector: ".umb-block-grid__area", // (Optional) Used for extended boundary between containers.
                containerSelector: ".umb-block-grid__layout-container", // Used for connecting with others
                itemSelector: ".umb-block-grid__layout-item",
                draggableSelector: ".umb-block-grid__block--view",
                placeholderClass: "umb-block-grid__layout-item-placeholder",
                ghostClass: "umb-block-grid__layout-item-ghost",
                onStart: onSortStart,
                onEnd: onSortEnd,
                onContainerChange: onSortContainerChange,
                onSync: onSortSync,
                onDisallowed: onSortDisallowed,
                onAllowed: onSortAllowed,
                onRequestDrop: onSortRequestDrop
            }
        }

        var currentItemColumnSpanTarget;
        function onSortStart(data) {
            currentItemColumnSpanTarget = data.item.columnSpan;

            // Gather containedPropertyEditorProxies from this element.
            currentContainedPropertyEditorProxies = Array.from(data.element.querySelectorAll('slot[data-is-property-editor-proxy]')).map(x => x.getAttribute('name'));
            vm.blockEditorApi.internal.startDraggingMode();
        }

        function onSortEnd() {
            vm.blockEditorApi.internal.exitDraggingMode();
            currentContainedPropertyEditorProxies = [];
            notifyVisualUpdate();
            $scope.$evalAsync();
        }

        function getColumnSpanForContext(currentColumnSpan, columnSpanOptions, contextColumns) {
            if (columnSpanOptions.length > 0) {
                const availableOptions = columnSpanOptions.filter(option => option.columnSpan <= contextColumns);
                if(availableOptions.length > 0) {
                    const closestColumnSpan = availableOptions.map(x => x.columnSpan).reduce(
                        (prev, curr) => {
                            return Math.abs(curr - currentColumnSpan) < Math.abs(prev - currentColumnSpan) ? curr : prev
                        }, 99999
                    );
                    if(closestColumnSpan) {
                        return closestColumnSpan;
                    }
                }
            }
            return contextColumns;
        }

        function onSortContainerChange(data) {
            const contextColumns = vm.blockEditorApi.internal.getContextColumns(data.ownerVM.parentBlock, data.ownerVM.areaKey);
            data.item.columnSpan = getColumnSpanForContext(currentItemColumnSpanTarget, data.item.$block.config.columnSpanOptions, contextColumns);
        }

        function onSortSync(data) {
            if (data.fromController !== data.toController) {
                removeAllContainedPropertyEditorProxies();
            }
            $scope.$evalAsync();
            vm.blockEditorApi.internal.setDirty();
        }

        function onSortDisallowed() {
            vm.showNotAllowedUI = true;
            $scope.$evalAsync();
        }
        function onSortAllowed() {
            vm.showNotAllowedUI = false;
            $scope.$evalAsync();
        }
        function onSortRequestDrop(data) {
            return vm.blockEditorApi.internal.isElementTypeKeyAllowedAt(vm.parentBlock, vm.areaKey, data.item.$block.config.contentElementTypeKey);
        }








        $scope.$on('$destroy', function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });
    }

})();
