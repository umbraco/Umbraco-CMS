(function () {
    "use strict";

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
                areaKey: "<?",
                parentBlock: "<?",
                parentForm: "<?",
                propertyEditorForm: "<?",
                depth: "@"
            }
        }
    );

    const ANIMATION_DURATION = 120;

    function BlockGridEntriesController($element, $scope, $timeout) {

        var vm = this;
        vm.showNotAllowedUI = false;
        vm.invalidAmount = false;
        vm.areaConfig = null;
        vm.locallyAvailableBlockTypes = 0;
        vm.invalidBlockTypes = [];

        vm.movingLayoutEntry = null;

        vm.$onInit = function () {
            initializeSortable();

            if(vm.parentBlock) {
                vm.areaConfig = vm.parentBlock.config.areas.find(area => area.key === vm.areaKey);
            }
            
            vm.locallyAvailableBlockTypes = vm.blockEditorApi.internal.getAllowedTypesOf(vm.parentBlock, vm.areaKey);
            
            $scope.$watch('vm.entries', onLocalAmountOfBlocksChanged, true);
        };

        function onLocalAmountOfBlocksChanged() {

            if (vm.parentForm && vm.areaConfig) {

                
                var isMinRequirementGood = vm.entries.length >= vm.areaConfig.minAllowed;
                vm.parentForm.areaMinCount.$setValidity("areaMinCount", isMinRequirementGood);

                var isMaxRequirementGood = vm.areaConfig.maxAllowed == null || vm.entries.length <= vm.areaConfig.maxAllowed;
                vm.parentForm.areaMaxCount.$setValidity("areaMaxCount", isMaxRequirementGood);

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
                vm.parentForm.areaTypeRequirements.$setValidity("areaTypeRequirements", isTypeRequirementGood);


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
            var _lastContainerVM = null;

            var targetRect = null;
            var relatedEl = null;
            var ghostEl = null;
            var ghostRect = null;
            var relatedRect = null;
            var dragX = 0;
            var dragY = 0;
            var dragOffsetX = 0;
            var dragOffsetY = 0;

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

            var awaitingContainer = null;
            var approvedContainer = gridLayoutContainerEl;
            var approvedContainerDate = null;

            var awaitingTarget = null;
            var approvedTarget = {related: null, after: false};
            var timeout = null;

            function _checkReset() {
                awaitingContainer = null;
                approvedContainer = null;
                //preventAwaitingNewContainer = false;

                awaitingTarget = null;
                approvedTarget = {related: null, after: false};
                clearTimeout(timeout);

                timeout = null;
            }

            function _checkStart() {
                approvedContainer = gridLayoutContainerEl;
                approvedContainerDate = new Date().getTime();
            }

            function dist(x1,y1,x2,y2){
                return Math.sqrt((x2-x1)*(x2-x1)+(y2-y1)*(y2-y1)); 
            }

            //var preventAwaitingNewContainer = false;
            function _checkMove(evt) {

                if(approvedTarget && evt.related === approvedTarget.related && evt.willInsertAfter === approvedTarget.after) {
                    return true;
                }
                
                // if cursor is within the ghostBox, then a move will be prevented:
                if(dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom) {
                    //console.log("Reject by ghostRect")
                    return false;
                }

                var contextVM = vm;
                if (gridLayoutContainerEl !== evt.to) {
                    contextVM = evt.to['Sortable:controller']();
                }

                // Skip waiting if its an empty container:
                const isEmptyContainer = (contextVM.entries.length === 0);

                if(evt.to !== approvedContainer) {
                    // If its only a few milliseconds since we approved the current container, then we will reject the new container:
                    const timeSinceApproval = new Date().getTime() - approvedContainerDate;
                    const rejectionTimeLeft = (ANIMATION_DURATION + 100) - timeSinceApproval;
                    if(rejectionTimeLeft > 0) {
                        //console.log("Reject by rejectionTimeLeft")
                        return false;
                    }
                    if(awaitingContainer === null) {//&& preventAwaitingNewContainer !== true
                        /*if(isEmptyContainer) {
                            clearTimeout(timeout);
                            timeout = null;
                            awaitingTarget = null;
                            awaitingContainer = null;
                            approvedContainer = evt.to;
                            // continuing..
                        } else {*/
                            awaitingTarget = null;
                            awaitingContainer = null;
                            awaitingContainer = evt.to;
                            clearTimeout(timeout);
                            timeout = setTimeout(_approveAwaitingContainer, ANIMATION_DURATION);
                            //console.log("Reject by setting new awaitingContainer")
                            return false;
                        //}
                    } else {
                        //console.log("Reject by awaitingContainer")
                        return false;
                    }
                }

                if(awaitingTarget === null) {

                    // Skip waiting if its an empty container:
                    if(isEmptyContainer) {
                        approvedTarget = {related: evt.related, after: evt.willInsertAfter};
                        awaitingTarget = null;
                        clearTimeout(timeout);
                        timeout = null;
                        return true;
                    }


                    // Measure distance from related element, this is not accurate as we don't know how the layout will turn out, but we need to do something to prevent the UI from trying out too possibilities, as it will flip around weirdly.
                    const dragRect = {
                        left: dragX - dragOffsetX,
                        top: dragY - dragOffsetY,
                        right: dragX - dragOffsetX + ghostRect.width,
                        bottom: dragY - dragOffsetY + ghostRect.height
                    }
                    /*let oldDistance = Math.min(
                        dist(dragRect.left, dragRect.top, ghostRect.left, ghostRect.top),
                        dist(dragRect.right, dragRect.top, ghostRect.right,ghostRect.top),
                        dist(dragRect.left, dragRect.bottom, ghostRect.left,ghostRect.bottom),
                        dist(dragRect.right, dragRect.bottom, ghostRect.right,ghostRect.bottom)
                    );
                    let newDistance = 0;
                    */
                    /*newDistance = Math.min(
                        dist(dragRect.left, dragRect.top, evt.relatedRect.left, evt.relatedRect.top),
                        dist(dragRect.right, dragRect.top, evt.relatedRect.right,evt.relatedRect.top),
                        dist(dragRect.left, dragRect.bottom, evt.relatedRect.left,evt.relatedRect.bottom),
                        dist(dragRect.right, dragRect.bottom, evt.relatedRect.right,evt.relatedRect.bottom)
                    );*/
                    /*
                    if(evt.willInsertAfter) {
                        newDistance = Math.min(
                            dist(dragRect.left, dragRect.top, evt.relatedRect.left, evt.relatedRect.bottom),
                            dist(dragRect.right, dragRect.top, evt.relatedRect.right, evt.relatedRect.bottom),

                            dist(dragRect.left, dragRect.top, evt.relatedRect.right, evt.relatedRect.top),
                            dist(dragRect.left, dragRect.bottom, evt.relatedRect.right, evt.relatedRect.bottom)
                        );
                    } else {
                        newDistance = Math.min(
                            dist(dragRect.left, dragRect.bottom, evt.relatedRect.left, evt.relatedRect.top),
                            dist(dragRect.right, dragRect.bottom, evt.relatedRect.right, evt.relatedRect.top),

                            dist(dragRect.right, dragRect.top, evt.relatedRect.left, evt.relatedRect.top),
                            dist(dragRect.right, dragRect.bottom, evt.relatedRect.left, evt.relatedRect.bottom)
                        );
                    }
                    */
                    let isCursorGood = false;
                    const distanceFromTopLeft = dist(dragX, dragY, evt.relatedRect.left, evt.relatedRect.top);
                    const distanceFromBottomRight = dist(dragX, dragY, evt.relatedRect.right, evt.relatedRect.bottom);
                    if(evt.willInsertAfter) {
                        /*
                        oldDistance = Math.min(
                            dist(dragRect.left, dragRect.top, ghostRect.left, ghostRect.top),
                            dist(dragRect.right, dragRect.bottom, ghostRect.right,ghostRect.bottom)
                        );
                        newDistance = Math.min(
                            dist(dragRect.right, dragRect.top, evt.relatedRect.right,evt.relatedRect.top),
                            dist(dragRect.right, dragRect.bottom, evt.relatedRect.right,evt.relatedRect.bottom)
                        );
                        */
                        isCursorGood = distanceFromTopLeft > distanceFromBottomRight;
                    } else {
                        /*
                        oldDistance = Math.min(
                            dist(dragRect.left, dragRect.top, ghostRect.left,ghostRect.top),
                            dist(dragRect.right, dragRect.bottom, ghostRect.right,ghostRect.bottom)
                        );
                        newDistance = Math.min(
                            dist(dragRect.left, dragRect.top, evt.relatedRect.left, evt.relatedRect.top),
                            dist(dragRect.left, dragRect.bottom, evt.relatedRect.left,evt.relatedRect.bottom)
                        );
                        */
                        isCursorGood = distanceFromTopLeft <= distanceFromBottomRight;
                    }

                    //console.log("calc", ghostRect.left,  ghostRect.top, "  |  ", dragRect.left, dragRect.top, "              calc: ", oldDistanceY, " < ", newDistanceY);
                    //&& oldDistance <= newDistance

                    // Reason: We judge if the drop is good, cause SortableJS out of the box would switch between before/after position very fast, instead we only allow either when the cursor is closest to the top/left -> before or bottom/right -> after.
                    if(!isCursorGood) {
                        //console.log("rejected because existing is closer", !isCursorGood, oldDistance <= newDistance, evt.willInsertAfter);
                        return false;
                    }

                    awaitingTarget = {related: evt.related, after: evt.willInsertAfter};
                    clearTimeout(timeout);
                    timeout = setTimeout(_approveAwaitingRelated, ANIMATION_DURATION);
                }
                //console.log("awaitingTarget reject")
                return false;
            }
            
            function _approveAwaitingContainer() {
                approvedContainer = awaitingContainer;
                approvedContainerDate = new Date().getTime();
                awaitingContainer = null;
                timeout = null;
                //_callOnMove();
            }
            function _approveAwaitingRelated() {
                approvedTarget = awaitingTarget;
                awaitingTarget = null;
                timeout = null;
                //_callOnMove();
            }

            function _indication(evt) {

                // If not the same gridLayoutContainerEl, then test for transfer option:

                var contextVM = vm;
                if (gridLayoutContainerEl !== evt.to) {
                    contextVM = evt.to['Sortable:controller']();
                }

                const movingEl = evt.dragged;

                if(_lastContainerVM !== contextVM && _lastContainerVM !== null) {
                    _lastContainerVM.hideNotAllowed();
                    _lastContainerVM.revertIndicateDroppable();
                }
                _lastContainerVM = contextVM;
                
                if(contextVM.acceptBlock(movingEl.dataset.contentElementTypeKey) === true) {
                    _lastContainerVM.hideNotAllowed();
                    _lastContainerVM.indicateDroppable();// This block is accepted to we will indicate a good drop.
                    return true;
                }

                contextVM.showNotAllowed();// This block is not accepted to we will indicate that its not allowed.

                return false;
            }

            function _dropInTheVoid() {

                if(!ghostEl) {
                    return;
                }

                if(gridLayoutContainerEl.animated) {
                    console.log("gridLayoutContainerEl.animated", gridLayoutContainerEl.animated)
                    return;
                }
                
                ghostRect = ghostEl.getBoundingClientRect();
                relatedRect = relatedEl?.getBoundingClientRect();

                const insideGhost = dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom;
                // We do not necessary have a related element jet, if so we can conclude we are outside ist rectangle.
                const insideRelated = relatedRect ? (dragX > relatedRect.left && dragX < relatedRect.right && dragY > relatedRect.top && dragY < relatedRect.bottom) : false;
                //!insideGhost && 
                //if (!insideRelated) {
                    // We do not hover something meaningful, so lets try to find a solution:
                    
                    let onSameRow = [];
                    const containerElements = Array.from(gridLayoutContainerEl.children);
                    for (const el of containerElements) {
                        const elRect = el.getBoundingClientRect();
                        // gather elements on the same row.
                        if(dragY >= elRect.top && dragY <= elRect.bottom && el !== ghostEl) {
                            onSameRow.push({el: el, rect:elRect});
                        }
                    }

                    let lastDistance = 99999;
                    let foundRelatedEl = null;
                    let foundRelatedRect = null;
                    let placeAfter = false;
                    onSameRow.forEach( sameRow => {
                        const centerX = (sameRow.rect.left + (sameRow.rect.width*.5));
                        let leftDistance = dragX - centerX;
                        let rightDistance = centerX - dragX;
                        if(leftDistance > Math.max(rightDistance, 0) && leftDistance < lastDistance) {
                            foundRelatedEl = sameRow.el;
                            foundRelatedRect = sameRow.rect;
                            placeAfter = true;
                        } else if (rightDistance > 0 && rightDistance < lastDistance) {
                            foundRelatedEl = sameRow.el;
                            foundRelatedRect = sameRow.rect;
                            placeAfter = false;
                        }
                        console.log("dists: ", leftDistance, rightDistance, placeAfter)
                    });

                    //console.log("place ", placeAfter, "related to", foundRelatedEl);

                    if (foundRelatedEl === ghostEl) {
                        console.error("NO ghostEl was found!!! not good!!!");
                        return;
                    }

                    if (foundRelatedEl) {


                        /*
                        const distanceFromGhost = dist(dragX, dragY, ghostRect.left + ghostRect.width*.5, ghostRect.top + ghostRect.height*.5);
                        const distanceFromFound = dist(dragX, dragY, foundRelatedRect.left + foundRelatedRect.width*.5, foundRelatedRect.top + foundRelatedRect.height*.5);
                        
                        if(distanceFromGhost > distanceFromFound) {
                            console.log("Too far?")
                            return;
                        }
                        */
                        let isCursorGood = false;
                        const distanceFromTopLeft = dist(dragX, dragY, foundRelatedRect.left, foundRelatedRect.top);
                        const distanceFromBottomRight = dist(dragX, dragY, foundRelatedRect.right, foundRelatedRect.bottom);
                        if(placeAfter) {
                            isCursorGood = distanceFromTopLeft > distanceFromBottomRight;
                        } else {
                            isCursorGood = distanceFromTopLeft <= distanceFromBottomRight;
                        }
                        if(!isCursorGood) {
                            console.log("Too far?")
                            return;
                        }
                        
                        //const relatedElementUdi = foundRelatedEl.dataset.elementUdi;

                        //const movingEntry = vm.entries.splice(oldIndex, 1)[0];

                        //let newIndex = vm.entries.findIndex(x => x.contentUdi === relatedElementUdi);
                        let newIndex = containerElements.indexOf(foundRelatedEl);
                        console.log("indexes", newIndex, "   placeAfter:", placeAfter);
                        if (newIndex === -1) {
                            console.error("newIndex not found!!!");
                        }
                        const nextEl = containerElements[(placeAfter ? newIndex+1 : newIndex)];
                        if (nextEl) {
                            gridLayoutContainerEl.insertBefore(ghostEl, nextEl);
                        } else {
                            gridLayoutContainerEl.appendChild(ghostEl);
                        }

                        //gridLayoutContainerEl.insertBefore(nextSibling, ghostEl.nextSibling);
                        /*vm.entries.splice(newIndex, 0, movingEntry);
                        if (nextSibling.nodeType === Node.COMMENT_NODE) {
                            gridLayoutContainerEl.insertBefore(nextSibling, ghostEl.nextSibling);
                        }*/

                        //$scope.$evalAsync();
                    }
                //}
            }

            var rqaId = null
            function _onDragMouseMove(evt) {
                /** ignorer last drag event, comes as clientX === 0 and clientY === 0 */
                if(vm.movingLayoutEntry && targetRect && ghostRect && evt.clientX !== 0 && evt.clientY !== 0) {

                    dragX = evt.clientX;
                    dragY = evt.clientY;

                    
                    ghostRect = ghostEl.getBoundingClientRect();
                    relatedRect = relatedEl?.getBoundingClientRect();

                    const insideGhost = dragX > ghostRect.left && dragX < ghostRect.right && dragY > ghostRect.top && dragY < ghostRect.bottom;
                    const insideRelated = relatedRect ? (dragX > relatedRect.left && dragX < relatedRect.right && dragY > relatedRect.top && dragY < relatedRect.bottom) : false;

                    //if (!insideRelated) {
                        if(rqaId === null) {
                            requestAnimationFrame(_dropInTheVoid);
                            rqaId = setTimeout(() => {rqaId = null;}, 150);
                        }
                    //}


                    const oldForceLeft = vm.movingLayoutEntry.forceLeft;
                    const oldForceRight = vm.movingLayoutEntry.forceRight;
                    var newValue = (dragX - dragOffsetX < targetRect.left - 20);
                    if(newValue !== oldForceLeft) {
                        vm.movingLayoutEntry.forceLeft = newValue;
                        if(oldForceRight) {
                            vm.movingLayoutEntry.forceRight = false;
                        }
                        vm.blockEditorApi.internal.setDirty();
                        vm.movingLayoutEntry.$block.__scope.$evalAsync();// needed for the block to be updated
                        $scope.$evalAsync();
                    }

                    newValue = (dragX - dragOffsetX + ghostRect.width > targetRect.right + 20) && (vm.movingLayoutEntry.forceLeft !== true);
                    if(newValue !== oldForceRight) {
                        vm.movingLayoutEntry.forceRight = newValue;
                        vm.blockEditorApi.internal.setDirty();
                        vm.movingLayoutEntry.$block.__scope.$evalAsync();// needed for the block to be updated
                        $scope.$evalAsync();
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

                onStart: function (evt) {
                    nextSibling = evt.from === evt.item.parentNode ? evt.item.nextSibling : evt.clone.nextSibling;

                    var contextVM = vm;
                    if (gridLayoutContainerEl !== evt.to) {
                        contextVM = evt.to['Sortable:controller']();
                    }
                    
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
                    dragOffsetX = evt.originalEvent.clientX - ghostRect.left;
                    dragOffsetY = evt.originalEvent.clientY - ghostRect.top;

                    window.addEventListener('drag', _onDragMouseMove);

                    //gridLayoutContainerEl.getRootNode().host.style.setProperty("--umb-block-grid--dragging-mode", 1);
                    document.documentElement.style.setProperty("--umb-block-grid--dragging-mode", 1);

                    _checkStart();
                    $scope.$evalAsync();
                },
                // Called by any change to the list (add / update / remove)
                onMove: function (evt) {
                    //console.log('onMove', evt)

                    relatedEl = evt.related;
                    relatedRect = evt.related.getBoundingClientRect();
                    targetRect = evt.to.getBoundingClientRect();
                    ghostRect = evt.draggedRect;
                    
                    if(_checkMove(evt) === false) {
                        return false;
                    }
                    // same properties as onEnd
                    if(_indication(evt) === false) {
                        return false;
                    }

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
                    console.log("# onAdd")
                    _sync(evt);
                    $scope.$evalAsync();
                },
                onUpdate: function (evt) {
                    console.log("# onUpdate", evt)
/*
                    const itemElementUdi = evt.item.dataset.elementUdi;
                    const currentIndex = vm.entries.findIndex(x => x.contentUdi === itemElementUdi);
                    if(evt.oldIndex !== currentIndex) {
                        console.error("SKIP SYNC")
                        evt.preventDefault();
                        evt.originalEvent.preventDefault();
                    } else {*/
                        _sync(evt);
                        $scope.$evalAsync();
                    //}
                },
                onEnd: function(evt) {
                    console.log("# onEnd")
                    window.removeEventListener('drag', _onDragMouseMove);

                    // ensure not-allowed indication is removed.
                    if(_lastContainerVM) {
                        _lastContainerVM.hideNotAllowed();
                        _lastContainerVM.revertIndicateDroppable();
                        _lastContainerVM = null;
                    }

                    _checkReset();

                    vm.movingLayoutEntry = null;
                    targetRect = null;
                    ghostRect = null;
                    relatedRect = null;
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

                // Called when dragging element changes position
                onChange: function(evt) {
                    evt.newIndex // most likely why this event is used is to get the dragging element's current index
                    // same properties as onEnd
                }
                */
            });







            function _nearestDrop(evt) {
                /*
                console.log("____ _nearestDrop")
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
                }
                else {
                    vm.entries.splice(newIndex, 0, vm.entries.splice(oldIndex, 1)[0]);
                }
                */
            }
/*
            const sortableBackground = Sortable.create(gridLayoutContainerEl, {
                group: "uniqueGridEditorID",  // links groups with same name.
                animation: ANIMATION_DURATION,  // ms, animation speed moving items when sorting, `0` — without animation
                easing: "cubic-bezier(1, 0, 0, 1)", // Easing for animation. Defaults to null. See https://easings.net/ for examples.
                cancel: '',
                draggable: ".umb-block-grid__layout-item",  // Specifies which items inside the element should be draggable

                dragoverBubble: true,
                
                onStart: function (evt) {
                    console.log("Background start");
                },
                onAdd: function (evt) {
                    _nearestDrop(evt);
                    console.log("Background start");
                    $scope.$evalAsync();
                },
                onUpdate: function (evt) {
                    _nearestDrop(evt);
                    $scope.$evalAsync();
                },
                onEnd: function(evt) {
                    console.log("Background end");
                    //window.removeEventListener('drag', _onDragMouseMove);
                }
            });
*/


            $scope.$on('$destroy', function () {
                sortable.destroy()
                sortableBackground.destroy()
            });

        }
    }

})();
