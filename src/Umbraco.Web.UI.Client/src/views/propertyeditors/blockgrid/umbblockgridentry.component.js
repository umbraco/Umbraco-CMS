(function () {
    "use strict";


    /**
     * Helper method that takes a weight map and finds the index of the value.
     * @param {number} position - the value to find the index of.
     * @param {number[]} weights - array of numbers each representing the weight/length.
     * @returns {number} - the index of the weight that contains the accumulated value
     */
    function getIndexOfPositionInWeightMap(position, weights) {
        let i = 0, len = weights.length, calc = 0;
        while(i<len) {
            if(position > calc && position <= calc+weights[i]) {
                return i;
            }

            calc += weights[i];
            i++;
        }
        return -1;
    }

    function closestColumnSpanOption(target, map) {
        return map.reduce((a, b) => {
            let aDiff = Math.abs(a.columnSpan - target);
            let bDiff = Math.abs(b.columnSpan - target);
    
            if (aDiff == bDiff) {
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
                areaKey: "<"
            }
        }
    );

    function BlockGridEntryController($scope, $element) {

        const vm = this;

        // Block sizing functionality:
        let layoutContainer = null;
        let gridColumns = null;
        let gridRows = null;
        //let targetCol = null;
        //let targetRow = null;


        function getNewSpans(startX, startY, endX, endY) {

            // Get start col:
            const blockStartCol = getIndexOfPositionInWeightMap(startX, gridColumns);
            const blockStartRow = getIndexOfPositionInWeightMap(startY, gridRows);
            const blockEndCol = getIndexOfPositionInWeightMap(endX, gridColumns);
            const blockEndRow = getIndexOfPositionInWeightMap(endY, gridRows);

            let newColumnSpan = Math.max(blockEndCol-blockStartCol, 1);
            // Find nearest allowed Column:
            newColumnSpan = closestColumnSpanOption(newColumnSpan, vm.layoutEntry.$block.config.columnSpanOptions).columnSpan;

            const newRowSpan = Math.max(blockEndRow-blockStartRow, 1);
            return {'columnSpan': newColumnSpan, 'rowSpan': newRowSpan};
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

            const computedStyles = window.getComputedStyle(layoutContainer);

            gridColumns = computedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x));
            gridRows = computedStyles.gridTemplateRows.trim().split("px").map(x => Number(x));

            gridRows.push(100);
            gridRows.push(100);
            gridRows.push(100);

            // ensure all columns are there.
            // add a few extra rows, so there is something to extend too.

            // TODO: handle non-css-grid mode,
            // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
            // use item height divided by rowSpan to identify row heights.

        }
        vm.onMouseMove = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.offsetX;
            const endY = e.offsetY;

            const newSpans = getNewSpans(startX, startY, endX, endY);

            console.log("newSpans", newSpans.columnSpan, newSpans.rowSpan)
        }

        vm.onMouseUp = function(e) {

            const layoutContainerRect = layoutContainer.getBoundingClientRect();
            const layoutItemRect = $element[0].getBoundingClientRect();

            const startX = layoutItemRect.left - layoutContainerRect.left;
            const startY = layoutItemRect.top - layoutContainerRect.top;
            const endX = e.offsetX;
            const endY = e.offsetY;

            const newSpans = getNewSpans(startX, startY, endX, endY);

            console.log("newSpans", newSpans.columnSpan, newSpans.rowSpan)

            vm.layoutEntry.columnSpan = newSpans.columnSpan;
            vm.layoutEntry.rowSpan = newSpans.rowSpan;

            // Remove listeners:
            window.removeEventListener('mousemove', vm.onMouseMove);
            window.removeEventListener('mouseup', vm.onMouseUp);
            window.removeEventListener('mouseleave', vm.onMouseUp);

            // CLean up variables:
            layoutContainer = null;
            gridColumns = null;
            gridRows = null;
            //targetCol = null;
            //targetRow = null;
        }



        vm.scaleHandlerKeyUp = function($event) {

            let addCol = 0;
            let addRow = 0;

            console.log($event.originalEvent);

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
            vm.layoutEntry.rowSpan = Math.max(vm.layoutEntry.rowSpan + addRow, 1);

            $event.originalEvent.stopPropagation();
        }

    }   

})();
