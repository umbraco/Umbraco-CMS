(function () {
    "use strict";

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
        let targetCol = null;
        let targetRow = null;

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

            gridColumns = computedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x))
            gridRows = computedStyles.gridTemplateRows.trim().split("px").map(x => Number(x))

            // TODO: handle non-css-grid mode,
            // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
            // use item height divided by rowSpan to identify row heights.

        }
        vm.onMouseMove = function(e) {

            const localX = e.offsetX;
            const localY = e.offsetY;

            targetCol = findIndexOfArray(localX, gridColumns);
            targetRow = findIndexOfArray(localY, gridRows);

        }

        function findIndexOfArray(position, weights) {
            let i = 0, len = weights.length, calc = 0;
            while(i<len) {
                if(position > calc && position <= calc+weights[i]) {
                    return i;
                }

                calc += weights[i]
                i++;
            }
            return -1;
        }

        vm.onMouseUp = function(e) {
            console.log(targetCol, targetRow);

            console.log(layoutContainer, $element[0]);
            const targetRect = layoutContainer.getBoundingClientRect();
            const elementRect = $element[0].getBoundingClientRect();
            const elementX = elementRect.left - targetRect.left;
            const elementY = elementRect.top - targetRect.top;

            // TODO: Numbers are good now, need to use them to find currentCol / Row and then figure out the new size.
            console.log(elementX, elementY)
            console.log(e.offsetX, e.offsetY)

            // Remove listeners:
            window.removeEventListener('mousemove', vm.onMouseMove);
            window.removeEventListener('mouseup', vm.onMouseUp);
            window.removeEventListener('mouseleave', vm.onMouseUp);

            // CLean up variables:
            layoutContainer = null;
            gridColumns = null;
            gridRows = null;
            targetCol = null;
            targetRow = null;
        }

    }   

})();
