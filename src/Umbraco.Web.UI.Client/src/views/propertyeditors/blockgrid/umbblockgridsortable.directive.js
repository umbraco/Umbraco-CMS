(function () {
    'use strict';


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

    function isWithinRect(x, y, rect, modifier) {
        return (x > rect.left - modifier && x < rect.right + modifier && y > rect.top - modifier && y < rect.bottom + modifier);
    }



    function UmbBlockGridSorter() {

        // scope.config.identifier
        // scope.config.items


        // Take over native auto scroll
        /* TODOS:
            Transfer to another container

            Take over native auto scroll
            Indicate not allowed drop
        */

        function link(scope, element) {


            let observer = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutation) {
                    mutation.addedNodes.forEach(function(addedNode) {
                        if (addedNode.matches && addedNode.matches(scope.config.itemSelector)) {
                            setupItem(addedNode);
                        }
                    });
                    mutation.removedNodes.forEach(function(removedNode) {
                        if (removedNode.matches && removedNode.matches(scope.config.itemSelector)) {
                            destroyItem(removedNode);
                        }
                    });
                });
            });


            let vm = {};

            // TODO: Actually not used for anything at this point of time when im writting this:
            vm.elements = [];
            vm.items = scope.config.items;


            let containerEl = scope.config.containerSelector ? element[0].closest(scope.config.containerSelector) : containerEl[0];
            if (!containerEl) {
                console.error("Could not initialize umb block grid sorter.", element[0])
                return;
            }

            

            function init() {

                containerEl['umbBlockGridSorter:vm'] = () => {
                    return vm;
                };
                containerEl.addEventListener('dragover', preventDragOver);

                observer.observe(containerEl, {childList: true, subtree: false});
            }
            init();

            function preventDragOver(e) {
                e.preventDefault()
            }

            function setupItem(element) {
                if(vm.elements.indexOf(element) !== -1) {
                    return;
                }

                //console.log("setupItem", element.dataset.elementUdi, element.className)

                vm.elements.push(element);

                setupIgnorerElements(element);

                const dragElement = element.querySelector(scope.config.draggableSelector);
                dragElement.draggable = true;
                dragElement.addEventListener('dragstart', handleDragStart);
            }

            function destroyItem(element) {
                const elementIndex = vm.elements.indexOf(element);
                if(elementIndex === -1) {
                    return
                }

                //console.log("destroyItem", element.dataset.elementUdi, element.className)

                vm.elements.splice(elementIndex, 1);

                destroyIgnorerElements(element);

                const dragElement = element.querySelector(scope.config.draggableSelector);
                dragElement.removeEventListener('dragstart', handleDragStart);
            }

            function setupIgnorerElements(element) {
                scope.config.ignorerSelector.split(',').forEach(function (criteria) {
                    element.querySelectorAll(criteria.trim()).forEach(setupPreventEvent);
                });
            }
            function destroyIgnorerElements(element) {
                scope.config.ignorerSelector.split(',').forEach(function (criteria) {
                    element.querySelectorAll(criteria.trim()).forEach(destroyPreventEvent);
                });
            }
            function setupPreventEvent(element) {
                element.addEventListener('dragstart', preventEvent);
            }
            function destroyPreventEvent(element) {
                element.removeEventListener('dragstart', preventEvent);
            }
            function preventEvent(event) {
                event.preventDefault();
                event.stopPropagation();
            }





            let currentContainerElement = containerEl;
            let currentContainerVM = vm;

            let rqaId = null;
            let currentItem = null;
            let currentElement = null;
            let currentDragElement = null;
            let currentDragRect = null;
            let dragX = 0;
            let dragY = 0;

            function handleDragStart(event) {


                event.stopPropagation();

                const element = event.target.closest(scope.config.itemSelector);
                element.addEventListener('drag', handleDragMove);
                element.addEventListener('dragend', handleDragEnd);

                //console.log("handleDragStart", element.dataset.elementUdi);

                currentElement = element;
                currentDragElement = element.querySelector(scope.config.draggableSelector);
                currentDragRect = currentDragElement.getBoundingClientRect();
                currentItem = vm.items.find(entry => scope.config.compareElementToModel(element, entry));

                const mouseOffsetX = Math.round(event.clientX - currentDragRect.left); //x position within the element.
                const mouseOffsetY = Math.round(event.clientY - currentDragRect.top);  //y position within the element.
                element.style.transformOrigin = `${Math.round((mouseOffsetX/currentDragRect.width)*100)}% ${Math.round((mouseOffsetY/currentDragRect.height)*100)}%`;
                element.classList.add(scope.config.ghostClass);

                if (scope.config.dataTransferResolver) {
                    scope.config.dataTransferResolver(event.dataTransfer, currentItem);
                }

                if (scope.config.onStart) {
                    scope.config.onStart(currentItem);
                }

                
                event.dataTransfer.setDragImage(currentDragElement, mouseOffsetX, mouseOffsetY);

                // We must wait one frame before changing the look of the block.
                // TODO: should this be cancelable
                requestAnimationFrame(() => {
                    element.classList.remove(scope.config.ghostClass);
                    element.classList.add(scope.config.placeholderClass);
                });

            }
            
            function handleDragEnd(event) {

                event.stopPropagation();

                const element = event.target.closest(scope.config.itemSelector);
                element.removeEventListener('drag', handleDragMove);
                element.removeEventListener('dragend', handleDragEnd);

                element.classList.remove(scope.config.placeholderClass);

                currentContainerVM.sync(currentElement, vm);

                if (scope.config.onEnd) {
                    scope.config.onEnd(currentItem);
                }

                if(rqaId) {
                    cancelAnimationFrame(rqaId);
                }

                currentContainerElement = containerEl;
                currentContainerVM = vm;

                rqaId = null
                currentItem = null;
                currentElement = null;
                currentDragElement = null;
                currentDragRect = null;
                dragX = 0;
                dragY = 0;
                
            }

            function handleDragMove(event) {

                if(!currentElement) {
                    return;
                }
                event.stopPropagation();

                const clientX = (event.touches ? event.touches[0] : event).clientX;
                const clientY = (event.touches ? event.touches[1] : event).clientY;
                if(clientX !== 0 && clientY !== 0) {

                    if(dragX === clientX && dragY === clientY) {
                        return;
                    }
                    dragX = clientX;
                    dragY = clientY;
                    
                    currentDragRect = currentDragElement.getBoundingClientRect();
                    const insideCurrentRect = isWithinRect(dragX, dragY, currentDragRect, 0);
                    if (!insideCurrentRect) {
                        if(rqaId === null) {
                            rqaId = requestAnimationFrame(moveCurrentElement);
                        }
                    }
                }
            }

            

            function moveCurrentElement() {

                rqaId = null;
                if(!currentElement) {
                    return;
                }
                /*
                if(!currentContainerElement) {
                    console.error("Cancel cause had no currentContainerElement", currentContainerElement)
                    return;
                }
                */
                
                currentDragRect = currentDragElement.getBoundingClientRect();
                const insideCurrentRect = isWithinRect(dragX, dragY, currentDragRect);
                if (insideCurrentRect) {
                    return;
                }

                // If we have a boundarySelector, try it, if we didn't get anything fall back to currentContainerElement.
                var currentBoundaryElement = (scope.config.boundarySelector ? currentContainerElement.closest(scope.config.boundarySelector) : currentContainerElement) || currentContainerElement;

                var currentBoundaryRect = currentBoundaryElement.getBoundingClientRect();
                
                const currentContainerHasItems = currentContainerVM.items.filter(x => x !== currentItem).length > 0;

                // if empty we will be move likely to accept an item (add 20px to the bounding box)
                // If we have items we must be 10 within the container to accept the move.
                const offsetEdge = currentContainerHasItems ? -10 : 20;
                if(isWithinRect(dragX, dragY, currentBoundaryRect, offsetEdge)) {
                    // we are good...
                } else if (scope.config.containerSelector) {
                    var parentContainer = currentContainerElement.parentNode.closest(scope.config.containerSelector);
                    if(parentContainer) {
                        const parentContainerVM = parentContainer['umbBlockGridSorter:vm']();
                        if(parentContainerVM.sortGroupIdentifier === vm.sortGroupIdentifier) {
                            currentContainerElement = parentContainer;
                            currentContainerVM = parentContainerVM;
                        }
                    }
                }



                var currentContainerRect = currentContainerElement.getBoundingClientRect();
                

                // gather elements on the same row.
                let elementsInSameRow = [];
                for (const el of currentContainerVM.elements) {
                    const elRect = el.getBoundingClientRect();
                    // gather elements on the same row.
                    if(dragY >= elRect.top && dragY <= elRect.bottom && el !== currentElement) {
                        const dragElement = el.querySelector(scope.config.draggableSelector);
                        const dragElementRect = dragElement.getBoundingClientRect();
                        elementsInSameRow.push({el:el, elRect:elRect, dragRect:dragElementRect});
                    }
                }

                let lastDistance = 99999;
                let foundEl = null;
                let foundElRect = null;
                let foundElDragRect = null;
                let placeAfter = false;
                elementsInSameRow.forEach( sameRow => {
                    const centerX = (sameRow.dragRect.left + (sameRow.dragRect.width*.5));
                    let distance = Math.abs(dragX - centerX);
                    if(distance < lastDistance) {
                        foundEl = sameRow.el;
                        foundElRect = sameRow.elRect;
                        foundElDragRect = sameRow.dragRect;
                        lastDistance = Math.abs(distance);
                        placeAfter = dragX > centerX;
                    }
                });

                /*
                if (foundEl === currentElement) {
                    return;
                }
                */
               
                // We want to retrive the children of the container, everytime to ensure we got the right order and index
                const orderedContainerElements = Array.from(currentContainerElement.children);

                if (foundEl) {

                    //let newIndex = containerElements.indexOf(foundRelatedEl);
                    //let foundItem = vm.items.find(entry => scope.config.compareElementToModel(foundEl, entry));


                    // Ghost is already on same line and we are not hovering the related element?
                    const currentRectCenterY = currentDragRect.top + (currentDragRect.height*.5);
                    const isInsideFound = isWithinRect(dragX, dragY, foundElDragRect, 0);
                    

                    // TODO: find another more generic way to do this.
                    if (isInsideFound && 
                        scope.config.itemHasNestedContainersResolver ? scope.config.itemHasNestedContainersResolver(foundEl) : foundEl.querySelector(scope.config.containerSelector)) {
                        // If mouse is on top of an area, then make that the new approvedContainer?
                        const blockView = foundEl.querySelector('.umb-block-grid__block--view');// TODO: I guess we could skip this line.
                        const subLayouts = blockView.querySelectorAll('.umb-block-grid__layout-container');
                        for (const subLayoutEl of subLayouts) {

                            var subBoundaryElement = (scope.config.boundarySelector ? subLayoutEl.closest(scope.config.boundarySelector) : subLayoutEl) || subLayoutEl;
                            var subBoundaryRect = subBoundaryElement.getBoundingClientRect();

                            const subContainerHasItems = subLayoutEl.querySelector('.umb-block-grid__layout-item:not(.'+scope.config.placeholderClass+')');
                            // gather elements on the same row.
                            const subOffsetEdge = subContainerHasItems ? -10 : 20;
                            if(isWithinRect(dragX, dragY, subBoundaryRect, subOffsetEdge)) {
                                
                                var subVm = subLayoutEl['umbBlockGridSorter:vm']();
                                // TODO: check acceptance, maybe combine with indication or acceptable?.
                                //if(subVm.sortGroupIdentifier === vm.sortGroupIdentifier) {
                                    currentContainerElement = subLayoutEl;
                                    currentContainerVM = subVm;
                                    moveCurrentElement();
                                    return;
                                //}
                            }
                        }
                    }
                    
                    if (currentRectCenterY > foundElDragRect.top && currentRectCenterY < foundElDragRect.bottom && !isInsideFound) {
                        // Note: during conversion i'm not sure why I have written this line..
                        // TODO: check what this has of impact.
                        console.error("This case, why?, I havent seen this happen. So I think this should be removed.")
                        return;
                    }

                    /*
                    TODO: Make indication / accept flow..
                    const containerVM = currentContainerElement['Sortable:controller']();
                    if(_indication(containerVM, currentElement) === false) {
                        return;
                    }
                    */

                    let verticalDirection = false;
                    
                    // TODO: move calculations out so they can be persisted a bit longer?

                    /** We need some data about the grid to figure out if there is room to be placed next to the found element */
                    const approvedContainerComputedStyles = getComputedStyle(currentContainerElement);
                    const gridColumnGap = Number(approvedContainerComputedStyles.columnGap.split("px")[0]) || 0;
                    const gridColumnNumber = parseInt(approvedContainerComputedStyles.getPropertyValue("--umb-block-grid--grid-columns"), 10);

                    const foundElColumns = parseInt(foundEl.dataset.colSpan, 10);
                    const currentElementColumns = parseInt(currentElement.dataset.colSpan, 10);

                    // Get grid template:
                    const approvedContainerGridColumns = approvedContainerComputedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x)).filter(n => n > 0).map((n, i, list) => list.length === i ? n : n + gridColumnGap);

                    // ensure all columns are there.
                    // This will also ensure handling non-css-grid mode,
                    // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
                    let amountOfColumnsInWeightMap = approvedContainerGridColumns.length;
                    const amountOfUnknownColumns = gridColumnNumber-amountOfColumnsInWeightMap;
                    if(amountOfUnknownColumns > 0) {
                        let accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, approvedContainerGridColumns) || 0;
                        const layoutWidth = currentContainerRect.width;
                        const missingColumnWidth = (layoutWidth-accumulatedValue)/amountOfUnknownColumns;
                        while(amountOfColumnsInWeightMap++ < gridColumnNumber) {
                            approvedContainerGridColumns.push(missingColumnWidth);
                        }
                    }


                    const relatedStartX = foundElDragRect.left - currentContainerRect.left;
                    const relatedStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(relatedStartX, approvedContainerGridColumns));

                    // If the found related element does not have enough room after which for the current element, then we go vertical mode:
                    if(relatedStartCol + foundElColumns + currentElementColumns > gridColumnNumber) {
                        // TODO: Check is there a case where placeAfter = false where we would like to check the previous element in this case?
                        verticalDirection = true;
                    }
                    
                    if (verticalDirection) {
                        placeAfter = (dragY > foundElDragRect.top + (foundElDragRect.height*.5));
                    }

                    const foundElIndex = orderedContainerElements.indexOf(foundEl);
                    const placeAt = (placeAfter ? foundElIndex+1 : foundElIndex);

                    console.log("move", placeAt, orderedContainerElements.length, placeAfter ? " AFTER" : " before")
                    move(orderedContainerElements, placeAt);

                    return;
                }

                /*
                TODO: Implement indication / accept flow when not within container.
                // If above or below container, we will go first or last.
                const containerVM = currentContainerElement['Sortable:controller']();
                if(_indication(containerVM, currentElement) === false) {
                    return;
                }
                */
                if(dragY < currentContainerRect.top) {
                    move(orderedContainerElements, 0);
                } else if(dragY > currentContainerRect.bottom) {
                    move(orderedContainerElements, -1);
                }
            }

            function move(orderedContainerElements, newElIndex) {

                newElIndex = newElIndex === -1 ? orderedContainerElements.length : newElIndex;
                
                const placeBeforeElement = orderedContainerElements[newElIndex];
                if (placeBeforeElement) {
                    // We do not need to move this, if the element to be placed before is it self.
                    if(placeBeforeElement !== currentElement) {
                        currentContainerElement.insertBefore(currentElement, placeBeforeElement);
                    }
                } else {
                    currentContainerElement.appendChild(currentElement);
                }

                if(scope.config.onChange) {
                    scope.config.onChange();
                }
            }


            /** Removes an element from container and returns its items-data entry */
            vm.removeElement = function (element) {
                if(!element) {
                    return null;
                }
                const oldIndex = vm.items.findIndex(entry => scope.config.compareElementToModel(element, entry));
                if(oldIndex !== -1) {
                    return vm.items.splice(oldIndex, 1)[0];
                }
                return null;
            }

            vm.sync = function(element, fromVm) {

                const movingItem = fromVm.removeElement(element);
                if(!movingItem) {
                    return;
                }

                /** Find next element, to then find the index of that element in items-data, to use as a safe reference to where the item will go in our items-data.
                 * This enables the container to contain various other elements and as well having these elements change while sorting is occurring.
                 */

                // find next valid element (This assumes the next element in DOM is presented in items-data, aka. only moving one item between each sync)
                let nextEl;
                let loopEl = element;
                while(loopEl = loopEl.nextElementSibling) {
                    if(loopEl.matches && loopEl.matches(scope.config.itemSelector)) {
                        nextEl = loopEl;
                        break;
                    }
                }

                let newIndex = vm.items.length;
                if(nextEl) {
                    // We had a reference element, we want to get the index of it.
                    // This is problem if a item is being moved forward?
                    newIndex = vm.items.findIndex(entry => scope.config.compareElementToModel(nextEl, entry));
                }

                vm.items.splice(newIndex, 0, movingItem);

                if(fromVm !== vm) {
                    fromVm.notifySync(movingItem);
                }
                vm.notifySync(movingItem);
            }

            vm.notifySync = function(movingItem) {
                if(scope.config.onSync) {
                    scope.config.onSync(movingItem);
                }
            }

            



            scope.$on('$destroy', () => {
                console.log("On Destroy sortable")

                containerEl['umbBlockGridSorter:vm'] = null
                containerEl.removeEventListener('dragover', preventDragOver);

                // Destroy!
                // Considering story all elements, and run through to clean up.

                observer.disconnect();
                observer = null;
                containerEl = null;
                vm = null;
            });
        }

        var directive = {
            restrict: 'A',
            scope: {
                config: '=umbBlockGridSorter'
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbBlockGridSorter', UmbBlockGridSorter);

})();
