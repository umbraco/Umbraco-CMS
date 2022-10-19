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


    function isWithinRect(x, y, rect, modifier) {
        return (x > rect.left - modifier && x < rect.right + modifier && y > rect.top - modifier && y < rect.bottom + modifier);
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
            var ghostEl = null;
            var ghostRect = null;
            var dragX = 0;
            var dragY = 0;
            var dragOffsetX = 0;

            var ghostElIndicateForceLeft = null;
            var ghostElIndicateForceRight = null;

            var approvedContainerEl = null;

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
                    }
                    else {
                        prevEntries.splice(oldIndex, 1);
                    }
                    
                    vm.entries.splice(newIndex, 0, syncEntry);

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
                    
                }
                else {
                    vm.entries.splice(newIndex, 0, vm.entries.splice(oldIndex, 1)[0]);
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
                    console.error("Cancel cause had no approvedContainerEl", approvedContainerEl)
                    return;
                }
                
                ghostRect = ghostEl.getBoundingClientRect();

                const insideGhost = isWithinRect(dragX, dragY, ghostRect);
                if (insideGhost) {
                    return;
                }

                var approvedContainerRect = approvedContainerEl.getBoundingClientRect();

                const approvedContainerHasItems = approvedContainerEl.querySelector('.umb-block-grid__layout-item:not(.umb-block-grid__layout-item-placeholder)');
                if(!approvedContainerHasItems && isWithinRect(dragX, dragY, approvedContainerRect, 20) || approvedContainerHasItems && isWithinRect(dragX, dragY, approvedContainerRect, -10)) {
                    // we are good...
                } else {
                    var parentContainer = approvedContainerEl.parentNode.closest('.umb-block-grid__layout-container');
                    if(parentContainer) {

                        if(parentContainer['Sortable:controller']().sortGroupIdentifier === vm.sortGroupIdentifier) {
                            approvedContainerEl = parentContainer;
                            approvedContainerRect = approvedContainerEl.getBoundingClientRect();
                        }
                    }
                }

                // gather elements on the same row.
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

                if (foundRelatedEl === ghostEl) {
                    return;
                }

                if (foundRelatedEl) {


                    let newIndex = containerElements.indexOf(foundRelatedEl);

                    const foundRelatedElRect = foundRelatedEl.getBoundingClientRect();

                    // Ghost is already on same line and we are not hovering the related element?
                    const ghostCenterY = ghostRect.top + (ghostRect.height*.5);
                    const isInsideFoundRelated = isWithinRect(dragX, dragY, foundRelatedElRect, 0);
                    

                    if (isInsideFoundRelated && foundRelatedEl.classList.contains('--has-areas')) {
                        // If mouse is on top of an area, then make that the new approvedContainer?
                        const blockView = foundRelatedEl.querySelector('.umb-block-grid__block--view');
                        const subLayouts = blockView.querySelectorAll('.umb-block-grid__layout-container');
                        for (const subLayout of subLayouts) {
                            const subLayoutRect = subLayout.getBoundingClientRect();
                            const hasItems = subLayout.querySelector('.umb-block-grid__layout-item:not(.umb-block-grid__layout-item-placeholder)');
                            // gather elements on the same row.
                            if(!hasItems && isWithinRect(dragX, dragY, subLayoutRect, 20) || hasItems && isWithinRect(dragX, dragY, subLayoutRect, -10)) {
                                
                                var subVm = subLayout['Sortable:controller']();
                                if(subVm.sortGroupIdentifier === vm.sortGroupIdentifier) {
                                    approvedContainerEl = subLayout;
                                    _moveGhostElement();
                                    return;
                                }
                            }
                        }
                    }
                    
                    if (ghostCenterY > foundRelatedElRect.top && ghostCenterY < foundRelatedElRect.bottom && !isInsideFoundRelated) {
                        return;
                    }

                    const containerVM = approvedContainerEl['Sortable:controller']();
                    if(_indication(containerVM, ghostEl) === false) {
                        return;
                    }

                    let verticalDirection = false;
                    if (ghostEl.dataset.forceLeft) {
                        placeAfter = true;
                    } else if (ghostEl.dataset.forceRight) {
                        placeAfter = true;
                    } else {

                        // if the related element is forceLeft and we are in the left side, we will set vertical direction, to correct placeAfter.
                        if (foundRelatedEl.dataset.forceLeft && placeAfter === false) {
                            verticalDirection = true;
                        } else 
                        // if the related element is forceRight and we are in the right side, we will set vertical direction, to correct placeAfter.
                        if (foundRelatedEl.dataset.forceRight && placeAfter === true) {
                            verticalDirection = true;
                        } else {

                            // TODO: move calculations out so they can be persisted a bit longer?
                            //const approvedContainerRect = approvedContainerEl.getBoundingClientRect();
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
                        }
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

                    return;
                }

                // If above or below container, we will go first or last.
                const containerVM = approvedContainerEl['Sortable:controller']();
                if(_indication(containerVM, ghostEl) === false) {
                    return;
                }
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

            var rqaId = null
            function _onDragMove(evt) {

                const clientX = (evt.touches ? evt.touches[0] : evt).clientX;
                const clientY = (evt.touches ? evt.touches[1] : evt).clientY;
                if(vm.movingLayoutEntry && targetRect && ghostRect && clientX !== 0 && clientY !== 0) {

                    if(dragX === clientX && dragY === clientY) {
                        return;
                    }
                    dragX = clientX;
                    dragY = clientY;
                    
                    ghostRect = ghostEl.getBoundingClientRect();

                    const insideGhost = isWithinRect(dragX, dragY, ghostRect, 0);
                    
                    if (!insideGhost) {
                        if(rqaId === null) {
                            rqaId = requestAnimationFrame(_moveGhostElement);
                        }
                    }

                    
                    if(vm.movingLayoutEntry.columnSpan !== vm.layoutColumnsInt) {
                        
                        const oldForceLeft = vm.movingLayoutEntry.forceLeft;
                        const oldForceRight = vm.movingLayoutEntry.forceRight;

                        var newValue = (dragX < targetRect.left);
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

                        newValue = (dragX > targetRect.right) && (vm.movingLayoutEntry.forceLeft !== true);
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

            vm.sortGroupIdentifier = "BlockGridEditor_"+vm.blockEditorApi.internal.uniqueEditorKey;

            const sortable = Sortable.create(gridLayoutContainerEl, {
                group: vm.sortGroupIdentifier,
                sort: true,
                animation: 0,
                cancel: '',
                draggable: ".umb-block-grid__layout-item",
                ghostClass: "umb-block-grid__layout-item-placeholder",
                swapThreshold: .4,
                dragoverBubble: true,
                emptyInsertThreshold: 40,

                scrollSensitivity: 50,
                scrollSpeed: 16,
                scroll: true,
                forceAutoScrollFallback: true,

                onStart: function (evt) {
                    nextSibling = evt.from === evt.item.parentNode ? evt.item.nextSibling : evt.clone.nextSibling;

                    var contextVM = vm;
                    if (gridLayoutContainerEl !== evt.to) {
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

                    ghostEl = evt.item;

                    targetRect = evt.to.getBoundingClientRect();
                    ghostRect = ghostEl.getBoundingClientRect();

                    const clientX = (evt.originalEvent.touches ? evt.originalEvent.touches[0] : evt.originalEvent).clientX;
                    dragOffsetX = clientX - ghostRect.left;

                    window.addEventListener('drag', _onDragMove);
                    window.addEventListener('dragover', _onDragMove);

                    document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", 1);

                    $scope.$evalAsync();
                },
                // Called by any change to the list (add / update / remove)
                onMove: function (evt) {
                    relatedEl = evt.related;
                    targetRect = evt.to.getBoundingClientRect();
                    ghostRect = evt.draggedRect;
    
                    // Disable SortableJS from handling the drop, instead we will use our own.
                    return false;
                },
                // When an change actually was made, after drop has occurred:
                onSort: function (evt) {
                    vm.blockEditorApi.internal.setDirty();
                },
                
                onAdd: function (evt) {
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onUpdate: function (evt) {
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onEnd: function(evt) {
                    if(rqaId !== null) {
                        cancelAnimationFrame(rqaId);
                    }
                    window.removeEventListener('drag', _onDragMove);
                    window.removeEventListener('dragover', _onDragMove);
                    document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", 0);

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
                    ghostEl = null;
                    relatedEl = null;
                }
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
