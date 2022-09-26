(function () {
    "use strict";



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
                depth: "@"
            }
        }
    );

    function BlockGridEntriesController($element, $scope, $timeout) {

        const unsubscribe = [];
        const vm = this;
        vm.showNotAllowedUI = false;
        vm.invalidAmount = false;
        vm.areaConfig = null;
        vm.locallyAvailableBlockTypes = 0;
        vm.invalidBlockTypes = [];

        vm.movingLayoutEntry = null;
        vm.layoutColumnsInt = 0;

        vm.$onInit = function () {
            initializeSortable();

            if(vm.parentBlock) {
                vm.areaConfig = vm.parentBlock.config.areas.find(area => area.key === vm.areaKey);
            }
            
            vm.locallyAvailableBlockTypes = vm.blockEditorApi.internal.getAllowedTypesOf(vm.parentBlock, vm.areaKey);
            
            unsubscribe.push($scope.$watch('vm.entries', onLocalAmountOfBlocksChanged, true));
        };

        unsubscribe.push($scope.$watch("layoutColumns", (newVal, oldVal) => {
            vm.layoutColumnsInt = parseInt(vm.layoutColumns, 10);
        }));


        function onLocalAmountOfBlocksChanged() {

            if (vm.entriesForm && vm.areaConfig) {
                
                var isMinRequirementGood = vm.entries.length >= vm.areaConfig.minAllowed;
                vm.entriesForm.areaMinCount.$setValidity("areaMinCount", isMinRequirementGood);

                var isMaxRequirementGood = vm.areaConfig.maxAllowed == null || vm.entries.length <= vm.areaConfig.maxAllowed;
                vm.entriesForm.areaMaxCount.$setValidity("areaMaxCount", isMaxRequirementGood);

                vm.areaConfig.specifiedAllowance.forEach(allowance => {

                    vm.invalidBlockTypes = vm.invalidBlockTypes.filter(type => type.key !== allowance.elementTypeKey)
                    const amount = vm.entries.filter(entry => entry.$block.data.contentTypeKey === allowance.elementTypeKey).length;
                    const minAllowed = allowance.minAllowed || 0;
                    const maxAllowed = allowance.maxAllowed || 0;
                    
                    if(amount < minAllowed || (maxAllowed > 0 && amount > maxAllowed)) {
                        vm.invalidBlockTypes.push({
                            'key': allowance.elementTypeKey,
                            'name': vm.locallyAvailableBlockTypes.find(blockType => blockType.elementTypeModel.name).elementTypeModel.name,
                            'amount': amount,
                            'minRequirement': minAllowed,
                            'maxRequirement': maxAllowed
                        });
                    }
                });
                var isTypeRequirementGood = vm.invalidBlockTypes.length === 0;
                vm.entriesForm.areaTypeRequirements.$setValidity("areaTypeRequirements", isTypeRequirementGood);


                vm.invalidAmount = !isMinRequirementGood || !isMaxRequirementGood || !isTypeRequirementGood;

                $element.toggleClass("--invalid", vm.invalidAmount);
            }
        }

        vm.acceptBlock = function(contentTypeKey) {
            return vm.blockEditorApi.internal.isElementTypeKeyAllowedAt(vm.parentBlock, vm.areaKey, contentTypeKey);
        }

        vm.getLayoutEntryByIndex = function(index) {
            return vm.blockEditorApi.internal.getLayoutEntryByIndex(vm.parentBlock, vm.areaKey, index);
        }

        vm.showNotAllowed = function() {
            vm.showNotAllowedUI = true;
            $scope.$evalAsync();
        }
        vm.hideNotAllowed = function() {
            vm.showNotAllowedUI = false;
            $scope.$evalAsync();
        }

        var revertIndicateDroppableTimeout;
        vm.revertIndicateDroppable = function() {
            revertIndicateDroppableTimeout = $timeout(() => {
                vm.droppableIndication = false;
            }, 2000);
        }
        vm.indicateDroppable = function() {
            if (revertIndicateDroppableTimeout) {
                $timeout.cancel(revertIndicateDroppableTimeout);
                revertIndicateDroppableTimeout = null;
             }
            vm.droppableIndication = true;
            $scope.$evalAsync();
        }

        function initializeSortable() {

            const gridLayoutContainerEl = $element[0].querySelector('.umb-block-grid__layout-container');
            var _lastIndicationContainerVM = null;

            var targetRect = null;
            var relatedEl = null;
            //var relatedRect = null;
            var ghostEl = null;
            var ghostRect = null;
            var dragX = 0;
            var dragY = 0;
            var dragOffsetX = 0;
            //var dragOffsetY = 0;

            var ghostElIndicateForceLeft = null;
            var ghostElIndicateForceRight = null;

            var approvedContainerEl = null;
            var approvedContainerDate = null;

            // Setup DOM method for communication between sortables:
            gridLayoutContainerEl['Sortable:controller'] = () => {
                return vm;
            };

            var nextSibling;

            // Borrowed concept from, its not identical as more has been implemented: https://github.com/SortableJS/angular-legacy-sortablejs/blob/master/angular-legacy-sortable.js
            function _sync(evt) {

                const oldIndex = evt.oldIndex,
                      newIndex = evt.newIndex;

                // If not the same gridLayoutContainerEl, then test for transfer option:
                if (gridLayoutContainerEl !== evt.from) {
                    const fromCtrl = evt.from['Sortable:controller']();
                    const prevEntries = fromCtrl.entries;
                    const syncEntry = prevEntries[oldIndex];

                    // Perform the transfer:

                    if (Sortable.active && Sortable.active.lastPullMode === 'clone') {
                        syncEntry = Utilities.copy(syncEntry);
                        prevEntries.splice(Sortable.utils.index(evt.clone, sortable.options.draggable), 0, prevEntries.splice(oldIndex, 1)[0]);

                        /*
                        if (evt.from.contains(evt.clone)) {
                            evt.from.removeChild(evt.clone);
                        }
                        */
                    }
                    else {
                        prevEntries.splice(oldIndex, 1);
                    }

                    const contextColumns = vm.blockEditorApi.internal.getContextColumns(vm.parentBlock, vm.areaKey);

                    // if colSpan is lower than contextColumns, and we do have some columnSpanOptions:
                    if (syncEntry.columnSpan < contextColumns && syncEntry.$block.config.columnSpanOptions.length > 0) {
                        // then check if the colSpan is a columnSpanOption, if NOT then reset to contextColumns.
                        const found = syncEntry.$block.config.columnSpanOptions.find(option => option.columnSpan === syncEntry.columnSpan);
                        if(!found) {
                            syncEntry.columnSpan = contextColumns;
                        }
                    } else {
                        syncEntry.columnSpan = contextColumns;
                    }

                    if(syncEntry.columnSpan === contextColumns) {
                        // If we are full width, then reset forceLeft/right.
                        syncEntry.forceLeft = false;
                        syncEntry.forceRight = false;
                    }
                    
                    vm.entries.splice(newIndex, 0, syncEntry);

                    // I currently do not think below line is necessary as this is updated through angularJS. This was giving trouble/errors.
                    //evt.from.insertBefore(evt.item, nextSibling); // revert element
                    
                }
                else {
                    vm.entries.splice(newIndex, 0, vm.entries.splice(oldIndex, 1)[0]);

                    // TODO: I don't think this is necessary, I would like to prove it purpose:
                    // move ng-repeat comment node to right position:
                    /*if (nextSibling.nodeType === Node.COMMENT_NODE) {
                        evt.from.insertBefore(nextSibling, evt.item.nextSibling);
                    }*/
                }
            }

            function _indication(contextVM, movingEl) {

                if(_lastIndicationContainerVM !== contextVM && _lastIndicationContainerVM !== null) {
                    _lastIndicationContainerVM.hideNotAllowed();
                    _lastIndicationContainerVM.revertIndicateDroppable();
                }
                _lastIndicationContainerVM = contextVM;
                
                if(contextVM.acceptBlock(movingEl.dataset.contentElementTypeKey) === true) {
                    _lastIndicationContainerVM.hideNotAllowed();
                    _lastIndicationContainerVM.indicateDroppable();// This block is accepted to we will indicate a good drop.
                    return true;
                }

                contextVM.showNotAllowed();// This block is not accepted to we will indicate that its not allowed.

                return false;
            }

            function _moveGhostElement() {

                rqaId = null;
                if(!ghostEl) {
                    return;
                }
                if(!approvedContainerEl) {
                    return;
                }
                /*
                if(approvedContainer.animated) {
                    console.log("approvedContainer.animated", approvedContainer.animated)
                    return;
                }
                */
                
                ghostRect = ghostEl.getBoundingClientRect();
                //relatedRect = relatedEl?.getBoundingClientRect();

                const insideGhost = dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom;
                // We do not necessary have a related element jet, if so we can conclude we are outside ist rectangle.
                //const insideRelated = relatedRect ? (dragX > relatedRect.left && dragX < relatedRect.right && dragY > relatedRect.top && dragY < relatedRect.bottom) : false;
                //!insideGhost && 
                if (!insideGhost) {
                    // We do not hover something meaningful, so lets try to find a solution:
                    
                    let elementInSameRow = [];
                    const containerElements = Array.from(approvedContainerEl.children);
                    for (const el of containerElements) {
                        const elRect = el.getBoundingClientRect();
                        // gather elements on the same row.
                        if(dragY >= elRect.top && dragY <= elRect.bottom && el !== ghostEl) {
                            elementInSameRow.push({el: el, rect:elRect});
                        }
                    }

                    let lastDistance = 99999;
                    let foundRelatedEl = null;
                    let placeAfter = false;
                    elementInSameRow.forEach( sameRow => {
                        const centerX = (sameRow.rect.left + (sameRow.rect.width*.5));
                        let distance = Math.abs(dragX - centerX);
                        if(distance < lastDistance) {
                            foundRelatedEl = sameRow.el;
                            lastDistance = Math.abs(distance);
                            placeAfter = dragX > centerX;
                        }
                    });

                    //console.log("place ", placeAfter, "related to", foundRelatedEl);

                    if (foundRelatedEl === ghostEl) {
                        console.error("NO ghostEl was found!!! not good!!!");
                        return;
                    }

                    if (foundRelatedEl) {

                        let newIndex = containerElements.indexOf(foundRelatedEl);
                        if (newIndex === -1) {
                            console.error("newIndex not found!!!, this situation needs to be dealt with, TODO.");
                        }


                        const foundRelatedElRect = foundRelatedEl.getBoundingClientRect();

                        // Ghost is already on same line and we are not hovering the related element?
                        const ghostCenterY = ghostRect.top + (ghostRect.height*.5);
                        const isInsideFoundRelated = (dragX > foundRelatedElRect.left && dragX < foundRelatedElRect.right && dragY > foundRelatedElRect.top && dragY < foundRelatedElRect.bottom);
                        if (ghostCenterY > foundRelatedElRect.top && ghostCenterY < foundRelatedElRect.bottom && !isInsideFoundRelated) {
                            console.log("Ghost is already on same line and we are not hovering the related element?")
                            return;
                        }

                        let verticalDirection = false;
                        if (ghostEl.dataset.forceLeft) {
                            placeAfter = true;
                        } else if (ghostEl.dataset.forceRight) {
                            placeAfter = true;
                        } else {
                            //if(isInsideFoundRelated) {

                                // if the related element is forceLeft and we are in the left side, we will set vertical direction, to correct placeAfter.
                                if (foundRelatedEl.dataset.forceLeft && placeAfter === false) {
                                    verticalDirection = true;
                                } else 
                                // if the related element is forceRight and we are in the right side, we will set vertical direction, to correct placeAfter.
                                if (foundRelatedEl.dataset.forceRight && placeAfter === true) {
                                    verticalDirection = true;
                                } else {

                                    // TODO: move calculations out so they can be persisted a bit longer?
                                    const approvedContainerRect = approvedContainerEl.getBoundingClientRect();
                                    const approvedContainerComputedStyles = getComputedStyle(approvedContainerEl);
                                    const gridColumnNumber = parseInt(approvedContainerComputedStyles.getPropertyValue("--umb-block-grid--grid-columns"), 10);

                                    const relatedColumns = parseInt(foundRelatedEl.dataset.colSpan, 10);
                                    const ghostColumns = parseInt(ghostEl.dataset.colSpan, 10);

                                    // Get grid template:
                                    const approvedContainerGridColumns = approvedContainerComputedStyles.gridTemplateColumns.trim().split("px").map(x => Number(x)).filter(n => n > 0);

                                    // ensure all columns are there.
                                    // This will also ensure handling non-css-grid mode,
                                    // use container width divided by amount of columns( or the item width divided by its amount of columnSpan)
                                    let amountOfColumnsInWeightMap = approvedContainerGridColumns.length;
                                    const amountOfUnknownColumns = gridColumnNumber-amountOfColumnsInWeightMap;
                                    if(amountOfUnknownColumns > 0) {
                                        let accumulatedValue = getAccumulatedValueOfIndex(amountOfColumnsInWeightMap, approvedContainerGridColumns) || 0;
                                        const layoutWidth = approvedContainerRect.width;
                                        const missingColumnWidth = (layoutWidth-accumulatedValue)/amountOfUnknownColumns;
                                        while(amountOfColumnsInWeightMap++ < gridColumnNumber) {
                                            approvedContainerGridColumns.push(missingColumnWidth);
                                        }
                                    }


                                    const relatedStartX = foundRelatedElRect.left - approvedContainerRect.left;
                                    const relatedStartCol = Math.round(getInterpolatedIndexOfPositionInWeightMap(relatedStartX, approvedContainerGridColumns));

                                    if(relatedStartCol + relatedColumns + ghostColumns > gridColumnNumber) {
                                        verticalDirection = true;
                                    }

                                    /*
                                    If they fit, then we go horizontal? unless forceLeft/forceRight on both?

                                    If they don't fit we go vertical...
                                    */
                                }
                            //}
                        }
                        if (verticalDirection) {
                            placeAfter = (dragY > foundRelatedElRect.top + (foundRelatedElRect.height*.5));
                        }
                        

                        const nextEl = containerElements[(placeAfter ? newIndex+1 : newIndex)];
                        if (nextEl) {
                            approvedContainerEl.insertBefore(ghostEl, nextEl);
                        } else {
                            approvedContainerEl.appendChild(ghostEl);
                        }

                        return
                    }

                    // If above or below container, we will go first or last.
                    const approvedContainerRect = approvedContainerEl.getBoundingClientRect();
                    if(dragY < approvedContainerRect.top) {
                        const firstEl = containerElements[0];
                        if (firstEl) {
                            approvedContainerEl.insertBefore(ghostEl, firstEl);
                        } else {
                            approvedContainerEl.appendChild(ghostEl);
                        }
                    } else if(dragY > approvedContainerRect.bottom) {
                        approvedContainerEl.appendChild(ghostEl);
                    }
                }
            }

            var rqaId = null
            function _onDragMove(evt) {

                const clientX = (evt.touches ? evt.touches[0] : evt).clientX;
                const clientY = (evt.touches ? evt.touches[1] : evt).clientY;
                /** ignorer last drag event, comes as clientX === 0 and clientY === 0 */
                if(vm.movingLayoutEntry && targetRect && ghostRect && clientX !== 0 && clientY !== 0) {

                    if(dragX === clientX && dragY === clientY) {
                        return;
                    }
                    dragX = clientX;
                    dragY = clientY;


                    
                    ghostRect = ghostEl.getBoundingClientRect();
                    //relatedRect = relatedEl?.getBoundingClientRect();

                    const insideGhost = dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom;
                    //const insideRelated = relatedRect ? (dragX > relatedRect.left && dragX < relatedRect.right && dragY > relatedRect.top && dragY < relatedRect.bottom) : false;
                    
                    if (!insideGhost) {
                        if(rqaId === null) {
                            rqaId = requestAnimationFrame(_moveGhostElement);
                            //rqaId = setTimeout(() => {rqaId = null;}, 150);
                        }
                    }

                    
                    if(vm.movingLayoutEntry.columnSpan !== vm.layoutColumnsInt) {
                        
                        const oldForceLeft = vm.movingLayoutEntry.forceLeft;
                        const oldForceRight = vm.movingLayoutEntry.forceRight;

                        var newValue = (dragX - dragOffsetX < targetRect.left - 50);
                        if(newValue !== oldForceLeft) {
                            vm.movingLayoutEntry.forceLeft = newValue;
                            if(oldForceRight) {
                                vm.movingLayoutEntry.forceRight = false;
                                if(ghostElIndicateForceRight) {
                                    ghostEl.removeChild(ghostElIndicateForceRight);
                                    ghostElIndicateForceRight = null;
                                }
                            }
                            vm.blockEditorApi.internal.setDirty();
                            vm.movingLayoutEntry.$block.__scope.$evalAsync();// needed for the block to be updated
                            $scope.$evalAsync();

                            // Append element for indication, as angularJS lost connection:
                            if(newValue === true) {
                                ghostElIndicateForceLeft = document.createElement("div");
                                ghostElIndicateForceLeft.className = "indicateForceLeft";
                                ghostEl.appendChild(ghostElIndicateForceLeft);
                            } else if(ghostElIndicateForceLeft) {
                                ghostEl.removeChild(ghostElIndicateForceLeft);
                                ghostElIndicateForceLeft = null;
                            }
                        }

                        newValue = (dragX - dragOffsetX + ghostRect.width > targetRect.right + 50) && (vm.movingLayoutEntry.forceLeft !== true);
                        if(newValue !== oldForceRight) {
                            vm.movingLayoutEntry.forceRight = newValue;
                            vm.blockEditorApi.internal.setDirty();
                            vm.movingLayoutEntry.$block.__scope.$evalAsync();// needed for the block to be updated
                            $scope.$evalAsync();

                            // Append element for indication, as angularJS lost connection:
                            if(newValue === true) {
                                ghostElIndicateForceRight = document.createElement("div");
                                ghostElIndicateForceRight.className = "indicateForceRight";
                                ghostEl.appendChild(ghostElIndicateForceRight);
                            } else if(ghostElIndicateForceRight) {
                                ghostEl.removeChild(ghostElIndicateForceRight);
                                ghostElIndicateForceRight = null;
                            }
                        }
                    }
                }
            }

            // TODO: Generate a unique ID for each Editor, also unique across variants.
            const sortable = Sortable.create(gridLayoutContainerEl, {
                group: "uniqueGridEditorID",  // links groups with same name.
                sort: true,  // sorting inside list
                //delay: 0, // time in milliseconds to define when the sorting should start
                //delayOnTouchOnly: false, // only delay if user is using touch
                //touchStartThreshold: 0, // px, how many pixels the point should move before cancelling a delayed drag event
                //disabled: false, // Disables the sortable if set to true.
                //store: null,  // @see Store
                animation: 0, //ANIMATION_DURATION,  // ms, animation speed moving items when sorting, `0` — without animation
                easing: "cubic-bezier(1, 0, 0, 1)", // Easing for animation. Defaults to null. See https://easings.net/ for examples.
                //handle: "umb-block-grid-block",  // Drag handle selector within list items,
                cancel: '',
                //filter: ".ignore-elements",  // Selectors that do not lead to dragging (String or Function)
                //preventOnFilter: true, // Call `event.preventDefault()` when triggered `filter`
                draggable: ".umb-block-grid__layout-item",  // Specifies which items inside the element should be draggable

                //dataIdAttr: 'data-element-udi', // HTML attribute that is used by the `toArray()` method

                ghostClass: "umb-block-grid__layout-item-placeholder",  // Class name for the drop placeholder
                //chosenClass: "sortable-chosen",  // Class name for the chosen item
                //dragClass: "sortable-drag",  // Class name for the dragging item

                swapThreshold: .4, // Threshold of the swap zone
                //invertSwap: true, // Will always use inverted swap zone if set to true
                //invertedSwapThreshold: .55, // Threshold of the inverted swap zone (will be set to swapThreshold value by default)
                //direction: 'horizontal', // Direction of Sortable (will be detected automatically if not given)
                /*direction: function(evt, target, dragEl) {
                    //if (target !== null && target.className.includes('half-column') && dragEl.className.includes('half-column')) {
                    //    return 'horizontal';
                    //}
                    console.log(evt, target, dragEl)
                    return 'vertical';
                },*/

                //forceFallback: true,  // ignore the HTML5 DnD behavior and force the fallback to kick in

                //fallbackClass: "sortable-fallback",  // Class name for the cloned DOM Element when using forceFallback
                //fallbackOnBody: false,  // Appends the cloned DOM Element into the Document's Body
                //fallbackTolerance: 0, // Specify in pixels how far the mouse should move before it's considered as a drag.

                dragoverBubble: true,
                //removeCloneOnHide: true, // Remove the clone element when it is not showing, rather than just hiding it
                emptyInsertThreshold: 160, // px, distance mouse must be from empty sortable to insert drag element into it

                scrollSensitivity: 50,
                scrollSpeed: 16,
                scroll: true,
                forceAutoScrollFallback: true,

                onStart: function (evt) {
                    nextSibling = evt.from === evt.item.parentNode ? evt.item.nextSibling : evt.clone.nextSibling;

                    var contextVM = vm;
                    if (gridLayoutContainerEl !== evt.to) {
                        console.error("container was different on start??")
                        contextVM = evt.to['Sortable:controller']();
                    }

                    approvedContainerEl = evt.to;
                    
                    const oldIndex = evt.oldIndex;
                    vm.movingLayoutEntry = contextVM.getLayoutEntryByIndex(oldIndex);
                    if(vm.movingLayoutEntry.forceLeft ||  vm.movingLayoutEntry.forceRight) {
                        // if one of these where true before, then we made a change here:
                        vm.blockEditorApi.internal.setDirty();
                    }
                    vm.movingLayoutEntry.forceLeft = false;
                    vm.movingLayoutEntry.forceRight = false;
                    vm.movingLayoutEntry.$block.__scope.$evalAsync();// needed for the block to be updated

                    //targetEl = evt.to;
                    //cloneEl = evt.clone;
                    ghostEl = evt.item;

                    targetRect = evt.to.getBoundingClientRect();
                    ghostRect = ghostEl.getBoundingClientRect();

                    const clientX = (evt.originalEvent.touches ? evt.originalEvent.touches[0] : evt.originalEvent).clientX;
                    dragOffsetX = clientX - ghostRect.left;
                    //dragOffsetY = evt.originalEvent.clientY - ghostRect.top;

                    window.addEventListener('drag', _onDragMove);
                    window.addEventListener('dragover', _onDragMove);

                    //gridLayoutContainerEl.getRootNode().host.style.setProperty("--umb-block-grid--dragging-mode", 1);
                    document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", 1);

                    $scope.$evalAsync();
                },
                // Called by any change to the list (add / update / remove)
                onMove: function (evt) {
                    //console.log('onMove', evt)

                    relatedEl = evt.related;
                    //relatedRect = evt.related.getBoundingClientRect();
                    targetRect = evt.to.getBoundingClientRect();
                    ghostRect = evt.draggedRect;

                    // if cursor is within the ghostBox, then a move will be prevented:
                    if(dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom) {
                        return false;
                    }

                    var contextVM = vm;
                    if (gridLayoutContainerEl !== evt.to) {
                        contextVM = evt.to['Sortable:controller']();
                    }

                    // same properties as onEnd
                    if(_indication(contextVM, evt.dragged) === false) {
                        return false;
                    }

                    if(evt.to !== approvedContainerEl) {

                        if(approvedContainerDate) {
                            const timeSinceApproval = new Date().getTime() - approvedContainerDate;
                            const rejectionTimeLeft = 250 - timeSinceApproval;
                            if(rejectionTimeLeft > 0) {
                                //console.log("Reject by rejectionTimeLeft")
                                return false;
                            }
                        }

                        // We will let SortableJS do the move when switching to a new container, otherwise not.
                        approvedContainerEl = evt.to;
                        approvedContainerDate = new Date().getTime();
                        return true;
                    }
    
                    // Disable SortableJS from handling the drop, instead we will use our own.
                    return false;
                },
                
                /*
                // When runtime dragging around:
                onChange: function (evt) {
                    //console.log('onChange', evt)
                    //evt.oldIndex;  // element index within parent
                },
                */
                // When an change actually was made, after drop has occurred:
                onSort: function (evt) {
                    //console.log('onSort', evt)
                    //evt.oldIndex;  // element index within parent
                    vm.blockEditorApi.internal.setDirty();
                },
                
                onAdd: function (evt) {
                    //console.log("# onAdd")
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onUpdate: function (evt) {
                    //console.log("# onUpdate", evt)
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onEnd: function(evt) {
                    console.log("# onEnd");
                    if(rqaId !== null) {
                        cancelAnimationFrame(rqaId);
                    }
                    window.removeEventListener('drag', _onDragMove);
                    window.removeEventListener('dragover', _onDragMove);

                    if(ghostElIndicateForceLeft) {
                        ghostEl.removeChild(ghostElIndicateForceLeft);
                        ghostElIndicateForceLeft = null;
                    }
                    if(ghostElIndicateForceRight) {
                        ghostEl.removeChild(ghostElIndicateForceRight);
                        ghostElIndicateForceRight = null;
                    }

                    // ensure not-allowed indication is removed.
                    if(_lastIndicationContainerVM) {
                        _lastIndicationContainerVM.hideNotAllowed();
                        _lastIndicationContainerVM.revertIndicateDroppable();
                        _lastIndicationContainerVM = null;
                    }

                    approvedContainerEl = null;
                    vm.movingLayoutEntry = null;
                    targetRect = null;
                    ghostRect = null;
                    //relatedRect = null;
                    relatedEl = null;
                }
                /*
                setData: function (dataTransfer, dragEl) {
                    dataTransfer.setData('Text', dragEl.textContent); // `dataTransfer` object of HTML5 DragEvent
                },

                // Element is chosen
                onChoose: function (evt) {
                    console.log('onChoose')
                    //evt.oldIndex;  // element index within parent
                },
                // Element is unchosen
                onUnchoose: function(evt) {
                    // same properties as onEnd
                },

                // Element dragging started
                onStart: function (evt) {
                    evt.oldIndex;  // element index within parent
                },

                // Element dragging ended
                onEnd: function (evt) {
                    var itemEl = evt.item;  // dragged HTMLElement
                    evt.to;    // target list
                    evt.from;  // previous list
                    evt.oldIndex;  // element's old index within old parent
                    evt.newIndex;  // element's new index within new parent
                    evt.oldDraggableIndex; // element's old index within old parent, only counting draggable elements
                    evt.newDraggableIndex; // element's new index within new parent, only counting draggable elements
                    evt.clone // the clone element
                    evt.pullMode;  // when item is in another sortable: `"clone"` if cloning, `true` if moving
                },

                // Element is dropped into the list from another list
                onAdd: function (evt) {
                    // same properties as onEnd
                },
                

                // Element is removed from the list into another list
                onRemove: function (evt) {
                    // same properties as onEnd
                },

                // Attempt to drag a filtered element
                onFilter: function (evt) {
                    var itemEl = evt.item;  // HTMLElement receiving the `mousedown|tapstart` event.
                },

                // Event when you move an item in the list or between lists
                onMove: function (evt, originalEvent) {
                    // Example: https://jsbin.com/nawahef/edit?js,output
                    evt.dragged; // dragged HTMLElement
                    evt.draggedRect; // DOMRect {left, top, right, bottom}
                    evt.related; // HTMLElement on which have guided
                    evt.relatedRect; // DOMRect
                    evt.willInsertAfter; // Boolean that is true if Sortable will insert drag element after target by default
                    originalEvent.clientY; // mouse position
                    // return false; — for cancel
                    // return -1; — insert before target
                    // return 1; — insert after target
                    // return true; — keep default insertion point based on the direction
                    // return void; — keep default insertion point based on the direction
                },

                // Called when creating a clone of element
                onClone: function (evt) {
                    var origEl = evt.item;
                    var cloneEl = evt.clone;
                },
                */
            });

            $scope.$on('$destroy', function () {
                sortable.destroy();
                for (const subscription of unsubscribe) {
                    subscription();
                }
            });

        };
    }

})();
