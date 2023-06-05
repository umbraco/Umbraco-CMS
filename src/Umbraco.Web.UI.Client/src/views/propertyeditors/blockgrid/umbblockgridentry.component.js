(function () {
    "use strict";


    /**
     * Helper method that takes a weight map and finds the index of the value.
     * @param {number} position - the value to find the index of.
     * @param {number[]} weights - array of numbers each representing the weight/length.
     * @returns {number} - the index of the weight that contains the accumulated value
     */
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
    
    function closestColumnSpanOption(target, map, max) {
        if(map.length > 0) {
            const result = map.reduce((a, b) => {
                if (a.columnSpan > max) {
                    return b;
                }
                let aDiff = Math.abs(a.columnSpan - target);
                let bDiff = Math.abs(b.columnSpan - target);
        
                if (aDiff === bDiff) {
                    return a.columnSpan < b.columnSpan ? a : b;
                } else {
                    return bDiff < aDiff ? b : a;
                }
            });
            if(result) {
                return result;
            }
        }
        return null;
    }



    /**
     * @ngdoc directive
     * @name umbraco.directives.directive:umbBlockGridEntry
     * @description
     * renders each row for the block grid editor
     */
    angular
        .module("umbraco")
        .component("umbBlockGridEntry", {
            templateUrl: 'views/propertyeditors/blockgrid/umb-block-grid-entry.html',
            controller: BlockGridEntryController,
            controllerAs: "vm",
            bindings: {
                blockEditorApi: "<",
                layoutColumns: "<",
                layoutEntry: "<",
                index: "<",
                parentBlock: "<",
                areaKey: "<",
                propertyEditorForm: "<?",
                depth: "@"
            },
            require: {
                umbBlockGridEntries: "?^^umbBlockGridEntries"
            }
        }
    );

    function BlockGridEntryController($scope, $element, $timeout) {

        let updateInlineCreateTimeout;
        let updateInlineCreateRaf;

        const unsubscribe = [];
        const vm = this;
        vm.areaGridColumns = '';
        vm.isHoveringArea = false;
        vm.isScaleMode = false;
        vm.layoutColumnsInt = 0;
        vm.inlineCreateAboveWidth = "";
        vm.hideInlineCreateAbove = true;
        vm.hideInlineCreateAfter = true;
        vm.canScale = false;

        vm.proxyProperties = [];
        vm.onAppendProxyProperty = (event) => {
            // Only insert a proxy slot for the direct Block of this entry (as all the blocks share the same ShadowDom though they are slotted into each other when nested through areas.)
            if (event.detail.contentUdi === vm.layoutEntry.contentUdi) {
                vm.proxyProperties.push({
                    slotName: event.detail.slotName
                });
                $scope.$evalAsync();
            }
        };
        vm.onRemoveProxyProperty = (event) => {
            // Only react to proxies from the direct Block of this entry:
            if (event.detail.contentUdi === vm.layoutEntry.contentUdi) {
                const index = vm.proxyProperties.findIndex(x => x.slotName === event.detail.slotName);
                if(index !== -1) {
                    vm.proxyProperties.splice(index, 1);
                }
                $scope.$evalAsync();
            }
        };

        vm.$onInit = function() {

            $element[0].addEventListener("UmbBlockGrid_AppendProperty", vm.onAppendProxyProperty);
            $element[0].addEventListener("UmbBlockGrid_RemoveProperty", vm.onRemoveProxyProperty);

            vm.childDepth = parseInt(vm.depth) + 1;

            if(vm.layoutEntry.$block.config.areaGridColumns) {
                vm.areaGridColumns = vm.layoutEntry.$block.config.areaGridColumns.toString();
            } else {
                vm.areaGridColumns = vm.blockEditorApi.internal.gridColumns.toString();
            }

            vm.layoutColumnsInt = parseInt(vm.layoutColumns, 10);

            vm.relevantColumnSpanOptions = vm.layoutEntry.$block.config.columnSpanOptions ? vm.layoutEntry.$block.config.columnSpanOptions.filter(x => x.columnSpan <= vm.layoutColumnsInt).sort((a,b) => (a.columnSpan > b.columnSpan) ? 1 : ((b.columnSpan > a.columnSpan) ? -1 : 0)) : [];
            const hasRelevantColumnSpanOptions = vm.relevantColumnSpanOptions.length > 1;
            const hasRowSpanOptions = vm.layoutEntry.$block.config.rowMinSpan && vm.layoutEntry.$block.config.rowMaxSpan && vm.layoutEntry.$block.config.rowMaxSpan !== vm.layoutEntry.$block.config.rowMinSpan;
            vm.canScale = (hasRelevantColumnSpanOptions || hasRowSpanOptions);
            
            unsubscribe.push(vm.layoutEntry.$block.__scope.$watch(() => vm.layoutEntry.$block.index, visualUpdateCallback));
            unsubscribe.push($scope.$on("blockGridEditorVisualUpdate", (evt, data) => {if(data.areaKey === vm.areaKey) { visualUpdateCallback()}}));

            updateInlineCreateTimeout = $timeout(updateInlineCreate, 500);

            $scope.$evalAsync();
        }
        unsubscribe.push($scope.$watch("depth", () => {
            vm.childDepth = parseInt(vm.depth) + 1;
        }));

        function visualUpdateCallback() {
            cancelAnimationFrame(updateInlineCreateRaf);
            updateInlineCreateRaf = requestAnimationFrame(updateInlineCreate);
        }
        
        /**
         * We want to only show the validation errors on the specific Block, not the parent blocks.
         * So we need to avoid having a Block as the parent to the Block Form.
         * Therefor we skip any parent blocks forms, and sets the parent form to the property editor.
         */
        vm.$postLink = function() {
            // If parent form is not the property editor form, then its another Block Forms and we will change it.
            if(vm.blockForm.$$parentForm !== vm.propertyEditorForm) {
                // Remove from parent block:
                vm.blockForm.$$parentForm.$removeControl(vm.blockForm);
                // Connect with property editor form:
                vm.propertyEditorForm.$addControl(vm.blockForm);
            }
        }
        vm.mouseOverArea = function(area) {
            if(area.items.length > 0) {
                vm.isHoveringArea = true;
            }
        }
        vm.mouseLeaveArea = function() {
            vm.isHoveringArea = false;
        }

        // Block sizing functionality:
        let layoutContainer = null;
        let gridColumns = null;
        let columnGap = 0;
        let rowGap = 0;
        let gridRows = null;
        let lockedGridRows = 0;
        let scaleBoxBackdropEl = null;
        let raf = null;

        function getNewSpans(startX, startY, endX, endY) {

            const blockStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(startX, gridColumns));
            const blockStartRow = Math.round(getInterpolatedIndexOfPositionInWeightMap(startY, gridRows));
            const blockEndCol = getInterpolatedIndexOfPositionInWeightMap(endX, gridColumns);
            const blockEndRow = getInterpolatedIndexOfPositionInWeightMap(endY, gridRows);

            let newColumnSpan = Math.max(blockEndCol-blockStartCol, 1);

            // Find nearest allowed Column:
            const bestColumnSpanOption = closestColumnSpanOption(newColumnSpan , vm.relevantColumnSpanOptions, vm.layoutColumnsInt - blockStartCol)
            newColumnSpan = bestColumnSpanOption ? bestColumnSpanOption.columnSpan : vm.layoutColumnsInt;

            let newRowSpan = Math.round(Math.max(blockEndRow-blockStartRow, vm.layoutEntry.$block.config.rowMinSpan || 1));
            if(vm.layoutEntry.$block.config.rowMaxSpan != null) {
                newRowSpan = Math.min(newRowSpan, vm.layoutEntry.$block.config.rowMaxSpan);
            }

            return {'columnSpan': newColumnSpan, 'rowSpan': newRowSpan, 'startCol': blockStartCol, 'startRow': blockStartRow};
        }

        function updateGridLayoutData(layoutContainerRect, layoutItemRect, updateRowTemplate) {

            const computedStyles = window.getComputedStyle(layoutContainer);


            columnGap = Number(computedStyles.columnGap.split("px")[0]) || 0;
            rowGap = Number(computedStyles.rowGap.split("px")[0]) || 0;

            gridColumns = computedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x));
            gridRows = computedStyles.gridTemplateRows.trim().split("px").map(x => Number(x));

            // remove empties:
            gridColumns = gridColumns.filter(n => n > 0);
            gridRows = gridRows.filter(n => n > 0);

            // We use this code to lock the templateRows, while scaling. otherwise scaling Rows is too crazy.
            if(updateRowTemplate || gridRows.length > lockedGridRows) {
                lockedGridRows = gridRows.length;
                layoutContainer.style.gridTemplateRows = computedStyles.gridTemplateRows;
            }

            // add gaps:
            const gridColumnsLen = gridColumns.length;
            gridColumns = gridColumns.map((n, i) => gridColumnsLen === i ? n : n + columnGap);
            const gridRowsLen = gridRows.length;
            gridRows = gridRows.map((n, i) => gridRowsLen === i ? n : n + rowGap);

            // ensure all columns are there.
            // This will also ensure handling non-css-grid mode,
            // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
            let amountOfColumnsInWeightMap = gridColumns.length;
            let gridColumnNumber = parseInt(computedStyles.getPropertyValue('--umb-block-grid--grid-columns'));
            const amountOfUnknownColumns = gridColumnNumber-amountOfColumnsInWeightMap;
            if(amountOfUnknownColumns > 0) {
                let accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, gridColumns) || 0;
                const layoutWidth = layoutContainerRect.width;
                const missingColumnWidth = (layoutWidth-accumulatedValue)/amountOfUnknownColumns;
                while(amountOfColumnsInWeightMap++ < gridColumnNumber) {
                    gridColumns.push(missingColumnWidth);
                }
            }


            // Handle non css grid mode for Rows:
            // use item height divided by rowSpan to identify row heights.
            if(gridRows.length === 0) {
                // Push its own height twice, to give something to scale with.
                gridRows.push(layoutItemRect.top - layoutContainerRect.top);

                let i = 0;
                const itemSingleRowHeight = layoutItemRect.height;
                while(i++ < vm.layoutEntry.rowSpan) {
                    gridRows.push(itemSingleRowHeight);
                }
            }

            // add a few extra rows, so there is something to extend too.
            // Add extra options for the ability to extend beyond current content:
            gridRows.push(50);
            gridRows.push(50);
            gridRows.push(50);
            gridRows.push(50);
            gridRows.push(50);
        }

        vm.scaleHandlerMouseDown = function($event) {
            $event.originalEvent.preventDefault();
            
            layoutContainer = $element[0].closest('.umb-block-grid__layout-container');
            if(!layoutContainer) {
                console.error($element[0], 'could not find parent layout-container');
                return;
            }

            vm.isScaleMode = true;
            
            window.addEventListener('mousemove', vm.onMouseMove);
            window.addEventListener('mouseup', vm.onMouseUp);
            window.addEventListener('mouseleave', vm.onMouseUp);

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();
            updateGridLayoutData(layoutContainerRect, layoutItemRect, true);

            
            scaleBoxBackdropEl = document.createElement('div');
            scaleBoxBackdropEl.className = 'umb-block-grid__scalebox-backdrop';
            layoutContainer.appendChild(scaleBoxBackdropEl);

        }
        vm.onMouseMove = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();


            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.clientX  - layoutContainerRect.left;
            const endY = e.clientY - layoutContainerRect.top;

            const newSpans = getNewSpans(startX, startY, endX, endY);

            const updateRowTemplate = vm.layoutEntry.columnSpan !== newSpans.columnSpan;

            if(updateRowTemplate) {
                // If we like to update we need to first remove the lock, make the browser render onces and then update.
                layoutContainer.style.gridTemplateRows = "";
            }
            cancelAnimationFrame(raf);
            raf = requestAnimationFrame(() => {
                // As mentioned above we need to wait until the browser has rendered DOM without the lock of gridTemplateRows.
                updateGridLayoutData(layoutContainerRect, layoutItemRect, updateRowTemplate);
            })

            // update as we go:
            vm.layoutEntry.columnSpan = newSpans.columnSpan;
            vm.layoutEntry.rowSpan = newSpans.rowSpan;

            $scope.$evalAsync();
        }

        vm.onMouseUp = function(e) {

            cancelAnimationFrame(raf);

            // Remove listeners:
            window.removeEventListener('mousemove', vm.onMouseMove);
            window.removeEventListener('mouseup', vm.onMouseUp);
            window.removeEventListener('mouseleave', vm.onMouseUp);


            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.clientX  - layoutContainerRect.left;
            const endY = e.clientY - layoutContainerRect.top;

            const newSpans = getNewSpans(startX, startY, endX, endY);


            // release the lock of gridTemplateRows:
            layoutContainer.removeChild(scaleBoxBackdropEl);
            layoutContainer.style.gridTemplateRows = "";
            vm.isScaleMode = false;

            // Clean up variables:
            layoutContainer = null;
            gridColumns = null;
            gridRows = null;
            lockedGridRows = 0;
            scaleBoxBackdropEl = null;
 
            // Update block size:
            vm.layoutEntry.columnSpan = newSpans.columnSpan;
            vm.layoutEntry.rowSpan = newSpans.rowSpan;

            vm.umbBlockGridEntries.notifyVisualUpdate();
            vm.blockEditorApi.internal.setDirty();
            $scope.$evalAsync();
        }



        vm.scaleHandlerKeyUp = function($event) {

            
            let addColIndex = 0;
            let addRow = 0;

            switch ($event.originalEvent.key) {
                case 'ArrowUp':
                    addRow = -1;
                    break;
                case 'ArrowDown':
                    addRow = 1;
                    break;
                case 'ArrowLeft':
                    addColIndex = -1;
                    break;
                case 'ArrowRight':
                    addColIndex = 1;
                    break;
            }

            if(addColIndex !== 0) {
                if (vm.relevantColumnSpanOptions.length > 0) {
                    const sortOptions = vm.relevantColumnSpanOptions;
                    const currentColIndex = sortOptions.findIndex(x => x.columnSpan === vm.layoutEntry.columnSpan);
                    const newColIndex = Math.min(Math.max(currentColIndex + addColIndex, 0), sortOptions.length-1);
                    vm.layoutEntry.columnSpan = sortOptions[newColIndex].columnSpan;
                } else {
                    vm.layoutEntry.columnSpan = vm.layoutColumnsInt;
                }
            }
            let newRowSpan = Math.max(vm.layoutEntry.rowSpan + addRow, vm.layoutEntry.$block.config.rowMinSpan || 1);
            if(vm.layoutEntry.$block.config.rowMaxSpan != null) {
                newRowSpan = Math.min(newRowSpan, vm.layoutEntry.$block.config.rowMaxSpan);
            }
            vm.layoutEntry.rowSpan = newRowSpan;

            vm.umbBlockGridEntries.notifyVisualUpdate();
            vm.blockEditorApi.internal.setDirty();
            $event.originalEvent.stopPropagation();
        }


        vm.clickInlineCreateAbove = function($event) {
            if(vm.hideInlineCreateAbove === false) {
                vm.blockEditorApi.requestShowCreate(vm.parentBlock, vm.areaKey, vm.index, $event);
            }
        }
        vm.clickInlineCreateAfter = function($event) {
            if(vm.hideInlineCreateAfter === false) {
                vm.blockEditorApi.requestShowCreate(vm.parentBlock, vm.areaKey, vm.index+1, $event, {'fitInRow': true});
            }
        }
        vm.mouseOverInlineCreate = function() {
            vm.blockEditorApi.internal.showAreaHighlight(vm.parentBlock, vm.areaKey);
        }
        vm.mouseOutInlineCreate = function() {
            vm.blockEditorApi.internal.hideAreaHighlight(vm.parentBlock, vm.areaKey);
        }
        
        function updateInlineCreate() {
            layoutContainer = $element[0].closest('.umb-block-grid__layout-container');
            if(!layoutContainer) {
                return;
            }

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            if(layoutContainerRect.width === 0) {
                $timeout.cancel(updateInlineCreateTimeout);
                vm.hideInlineCreateAbove = true;
                vm.hideInlineCreateAfter = true;
                vm.inlineCreateAboveWidth = "";
                $scope.$evalAsync();
                updateInlineCreateTimeout = $timeout(updateInlineCreate, 500);
                return;
            }

            if(layoutItemRect.right > layoutContainerRect.right - 5) {
                vm.hideInlineCreateAfter = true;
            } else {
                vm.hideInlineCreateAfter = false;
            }

            if(layoutItemRect.left > layoutContainerRect.left + 5) {
                vm.hideInlineCreateAbove = true;
                vm.inlineCreateAboveWidth = "";
            } else {
                vm.inlineCreateAboveWidth = getComputedStyle(layoutContainer).width;
                vm.hideInlineCreateAbove = false;
            }
            $scope.$evalAsync();
        }

        $scope.$on("$destroy", function () {

            $timeout.cancel(updateInlineCreateTimeout);

            $element[0].removeEventListener("UmbBlockGrid_AppendProperty", vm.onAppendProxyProperty);
            $element[0].removeEventListener("UmbBlockGrid_RemoveProperty", vm.onRemoveProxyProperty);

            for (const subscription of unsubscribe) {
                subscription();
            }
        });

    }   

})();
