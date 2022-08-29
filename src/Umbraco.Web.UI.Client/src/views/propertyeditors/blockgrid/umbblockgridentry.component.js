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
    
    function closestColumnSpanOption(target, map) {
        return map.reduce((a, b) => {
            let aDiff = Math.abs(a.columnSpan - target);
            let bDiff = Math.abs(b.columnSpan - target);
    
            if (aDiff === bDiff) {
                return a.columnSpan < b.columnSpan ? a : b;
            } else {
                return bDiff < aDiff ? b : a;
            }
        });
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
                layoutEntry: "<",
                index: "<",
                parentBlock: "<",
                areaKey: "<",
                depth: "@"
            }
        }
    );

    function BlockGridEntryController($scope, $element) {

        const vm = this;
        vm.areaGridStyles = {};
        vm.isHoveringArea = false;

        vm.$onInit = function() {

            vm.childDepth = parseInt(vm.depth) + 1;

            if(vm.layoutEntry.$block.config.areaGridColumns) {
                vm.areaGridStyles['--umb-block-grid--area-grid-columns'] = vm.layoutEntry.$block.config.areaGridColumns.toString();
            } else {
                vm.areaGridStyles['--umb-block-grid--area-grid-columns'] = 'initial';
            }
            $scope.$evalAsync();
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
        let gridRows = null;
        let scaleBoxEl = null;
        let scaleBoxBackdropEl = null;


        function getNewSpans(startX, startY, endX, endY) {

            const blockStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(startX, gridColumns));
            const blockStartRow = Math.round(getInterpolatedIndexOfPositionInWeightMap(startY, gridRows));
            const blockEndCol = getInterpolatedIndexOfPositionInWeightMap(endX, gridColumns);
            const blockEndRow = getInterpolatedIndexOfPositionInWeightMap(endY, gridRows);

            let newColumnSpan = Math.max(blockEndCol-blockStartCol, 1);
            // Find nearest allowed Column:
            newColumnSpan = closestColumnSpanOption(newColumnSpan , vm.layoutEntry.$block.config.columnSpanOptions).columnSpan;

            let newRowSpan = Math.round(Math.max(blockEndRow-blockStartRow, vm.layoutEntry.$block.config.rowMinSpan || 1));
            if(vm.layoutEntry.$block.config.rowMaxSpan != null) {
                newRowSpan = Math.min(newRowSpan, vm.layoutEntry.$block.config.rowMaxSpan);
            }

            return {'columnSpan': newColumnSpan, 'rowSpan': newRowSpan, 'startCol': blockStartCol, 'startRow': blockStartRow};
        }

        function updateGridLayoutData(layoutContainerRect, layoutItemRect) {

            const computedStyles = window.getComputedStyle(layoutContainer);

            gridColumns = computedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x));
            gridRows = computedStyles.gridTemplateRows.trim().split("px").map(x => Number(x));

            // remove empties:
            if(gridColumns[gridColumns.length-1] === 0) {
                gridColumns.pop();
            }
            if(gridRows[gridRows.length-1] === 0) {
                gridRows.pop();
            }
            // ensure all columns are there.
            // This will ensure handling non-css-grid mode,
            // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
            let amountOfColumnsInWeightMap = gridColumns.length;
            let gridColumnNumber = parseInt(computedStyles.getPropertyValue('--umb-block-grid--grid-columns'));
            const amountOfUnknownColumns = gridColumnNumber-amountOfColumnsInWeightMap;
            if(amountOfUnknownColumns > 0) {
                let accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, gridColumns);
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
                gridRows.push(layoutItemRect.height);
                gridRows.push(layoutItemRect.height);
            }

            // add a few extra rows, so there is something to extend too.
            // Add extra options for the ability to extend beyond current content:
            gridRows.push(50);
            gridRows.push(50);
            gridRows.push(50);
        }

        vm.scaleHandlerMouseDown = function($event) {
            $event.originalEvent.preventDefault();
            
            window.addEventListener('mousemove', vm.onMouseMove);
            window.addEventListener('mouseup', vm.onMouseUp);
            window.addEventListener('mouseleave', vm.onMouseUp);


            layoutContainer = $element[0].closest('.umb-block-grid__layout-container');
            if(!layoutContainer) {
                console.error($element[0], 'could not find parent layout-container');
            }

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();
            updateGridLayoutData(layoutContainerRect, layoutItemRect);

            scaleBoxBackdropEl = document.createElement('div');
            scaleBoxBackdropEl.className = 'umb-block-grid__scalebox-backdrop';
            layoutContainer.appendChild(scaleBoxBackdropEl);

            scaleBoxEl = document.createElement('div');
            scaleBoxEl.className = 'umb-block-grid__scalebox';

            const scaleBoxScaleHandler = document.createElement('button');
            scaleBoxScaleHandler.className = 'umb-block-grid__scale-handler';
            scaleBoxEl.appendChild(scaleBoxScaleHandler);

            $element[0].appendChild(scaleBoxEl);


        }
        vm.onMouseMove = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();
            updateGridLayoutData(layoutContainerRect, layoutItemRect);


            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.clientX  - layoutContainerRect.left;
            const endY = e.clientY - layoutContainerRect.top;

            const newSpans = getNewSpans(startX, startY, endX, endY);
            const endCol = newSpans.startCol + newSpans.columnSpan;
            const endRow = newSpans.startRow + newSpans.rowSpan;


            const startCellX =  getAccumulatedValueOfIndex(newSpans.startCol, gridColumns);
            const startCellY =  getAccumulatedValueOfIndex(newSpans.startRow, gridRows);
            const endCellX =  getAccumulatedValueOfIndex(endCol, gridColumns);
            const endCellY =  getAccumulatedValueOfIndex(endRow, gridRows);

            scaleBoxEl.style.width = Math.round(endCellX-startCellX)+'px';
            scaleBoxEl.style.height = Math.round(endCellY-startCellY)+'px';
            
            // update as we go:
            vm.layoutEntry.columnSpan = newSpans.columnSpan;
            vm.layoutEntry.rowSpan = newSpans.rowSpan;

            $scope.$evalAsync();
        }

        vm.onMouseUp = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.clientX  - layoutContainerRect.left;
            const endY = e.clientY - layoutContainerRect.top;

            const newSpans = getNewSpans(startX, startY, endX, endY);

            // Remove listeners:
            window.removeEventListener('mousemove', vm.onMouseMove);
            window.removeEventListener('mouseup', vm.onMouseUp);
            window.removeEventListener('mouseleave', vm.onMouseUp);

            layoutContainer.removeChild(scaleBoxBackdropEl);
            $element[0].removeChild(scaleBoxEl);

            // Clean up variables:
            layoutContainer = null;
            gridColumns = null;
            gridRows = null;
            scaleBoxEl = null;
            scaleBoxBackdropEl = null;

            // Update block size:
            vm.layoutEntry.columnSpan = newSpans.columnSpan;
            vm.layoutEntry.rowSpan = newSpans.rowSpan;
            $scope.$evalAsync();
        }



        vm.scaleHandlerKeyUp = function($event) {

            let addCol = 0;
            let addRow = 0;

            switch ($event.originalEvent.key) {
                case 'ArrowUp':
                    addRow = -1;
                    break;
                case 'ArrowDown':
                    addRow = 1;
                    break;
                case 'ArrowLeft':
                    addCol = -1;
                    break;
                case 'ArrowRight':
                    addCol = 1;
                    break;
            }

            const newColumnSpan = Math.max(vm.layoutEntry.columnSpan + addCol, 1);

            vm.layoutEntry.columnSpan = closestColumnSpanOption(newColumnSpan, vm.layoutEntry.$block.config.columnSpanOptions).columnSpan;
            let newRowSpan = Math.max(vm.layoutEntry.rowSpan + addRow, vm.layoutEntry.$block.config.rowMinSpan || 1);
            if(vm.layoutEntry.$block.config.rowMaxSpan != null) {
                newRowSpan = Math.min(newRowSpan, vm.layoutEntry.$block.config.rowMaxSpan);
            }
            vm.layoutEntry.rowSpan = newRowSpan;

            $event.originalEvent.stopPropagation();
        }

    }   

})();
