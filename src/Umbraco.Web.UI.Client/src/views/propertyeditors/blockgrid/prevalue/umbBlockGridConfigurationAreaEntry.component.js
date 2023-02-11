(function () {
    "use strict";

    /**
     * Note for new backoffice: there is a lot of similarities between the Area configuration and the Block entry, as they both share Grid scaling features.
     */


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
        });

        // Adding interpolation to the index, to get 
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
     * @name umbraco.directives.directive:umbBlockGridConfigurationAreaEntry
     * @description
     * Rendering the a specific area configuration for the block grid editor
     */
    angular
        .module("umbraco")
        .component("umbBlockGridConfigurationAreaEntry", {
            templateUrl: 'views/propertyeditors/blockgrid/prevalue/umb-block-grid-configuration-area-entry.html',
            controller: BlockGridConfigurationAreaEntryController,
            controllerAs: "vm",
            bindings: {
                area: "=",
                onEdit: "&",
                onDelete: "&"
            }
        }
    );

    function BlockGridConfigurationAreaEntryController($scope, $element) {

        const vm = this;

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

            const newColumnSpan = Math.round(Math.max(blockEndCol-blockStartCol, 1));
            const newRowSpan = Math.round(Math.max(blockEndRow-blockStartRow, 1));

            return {'columnSpan': newColumnSpan, 'rowSpan': newRowSpan, 'startCol': blockStartCol, 'startRow': blockStartRow};
        }

        function updateGridLayoutData() {

            if(!layoutContainer) {
                layoutContainer = $element[0].closest('.umb-block-grid-area-editor__grid-wrapper');
                if(!layoutContainer) {
                    console.error($element[0], 'could not find area-container');
                }
            }

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

            updateGridLayoutData();

            scaleBoxBackdropEl = document.createElement('div');
            scaleBoxBackdropEl.className = 'umb-block-grid-area-editor__scalebox-backdrop';
            layoutContainer.appendChild(scaleBoxBackdropEl);

            scaleBoxEl = document.createElement('div');
            scaleBoxEl.className = 'umb-block-grid-area-editor__scalebox';

            const scaleBoxScaleHandler = document.createElement('button');
            scaleBoxScaleHandler.className = 'umb-block-grid-area-editor__scale-handler';
            scaleBoxEl.appendChild(scaleBoxScaleHandler);

            $element[0].appendChild(scaleBoxEl);

        }
        vm.onMouseMove = function(e) {

            updateGridLayoutData();

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.pageX - layoutContainerRect.left;
            const endY = e.pageY - layoutContainerRect.top;

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
            vm.area.columnSpan = newSpans.columnSpan;
            vm.area.rowSpan = newSpans.rowSpan;

            $scope.$evalAsync();
        }

        vm.onMouseUp = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.pageX - layoutContainerRect.left;
            const endY = e.pageY - layoutContainerRect.top;

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
            vm.area.columnSpan = newSpans.columnSpan;
            vm.area.rowSpan = newSpans.rowSpan;
            $scope.$evalAsync();
        }



        vm.scaleHandlerKeyUp = function($event) {

            updateGridLayoutData();

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

            // Todo: Ensure value fit with configuration.
            vm.area.columnSpan = Math.min(Math.max(vm.area.columnSpan + addCol, 1), gridColumns.length);
            vm.area.rowSpan = Math.max(vm.area.rowSpan + addRow, 1);

            $event.originalEvent.stopPropagation();
        }



        vm.onEditClick = function($event) {
            $event.stopPropagation();
            vm.onEdit();
        }

        vm.onDeleteClick = function($event) {
            $event.stopPropagation();
            vm.onDelete();
        }

    }   

})();
