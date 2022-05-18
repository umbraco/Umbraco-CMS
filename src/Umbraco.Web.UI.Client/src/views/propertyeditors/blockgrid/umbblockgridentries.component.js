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
                parentBlock: "<?"
            }
        }
    );

    function BlockGridEntriesController($element, $scope) {

        var vm = this;

        vm.$onInit = function () {
            initializeSortable();
        };

        function initializeSortable() {

            const gridLayoutContainerEl = $element[0].querySelector('.umb-block-grid__layout-container');

            // Setup DOM method for communication between sortables:
            gridLayoutContainerEl['Sortable:ng-sortable'] = () => {
                return vm.entries;
            };

            var removed, nextSibling;

            // Borrowed concept from: https://github.com/SortableJS/angular-legacy-sortablejs/blob/master/angular-legacy-sortable.js
            // TODO: investigate usage of Store options prop.
            function _sync(evt) {
                var items = vm.entries;

                var oldIndex = evt.oldIndex,
                    newIndex = evt.newIndex;

                // If not the same layout array, then:
                if (gridLayoutContainerEl !== evt.from) {
                    var prevItems = evt.from['Sortable:ng-sortable']();

                    removed = prevItems[oldIndex];

                    if (Sortable.active && Sortable.active.lastPullMode === 'clone') {
                        removed = Utilities.copy(removed);
                        prevItems.splice(Sortable.utils.index(evt.clone, sortable.options.draggable), 0, prevItems.splice(oldIndex, 1)[0]);

                        if (evt.from.contains(evt.clone)) {
                            evt.from.removeChild(evt.clone);
                        }
                    }
                    else {
                        prevItems.splice(oldIndex, 1);
                    }

                    items.splice(newIndex, 0, removed);

                    // TODO: fix issue when dragging one level out.
                    evt.from.insertBefore(evt.item, nextSibling); // revert element
                }
                else {
                    items.splice(newIndex, 0, items.splice(oldIndex, 1)[0]);

                    // move ng-repeat comment node to right position:
                    if (nextSibling.nodeType === Node.COMMENT_NODE) {
                        evt.from.insertBefore(nextSibling, evt.item.nextSibling);
                    }
                }

                $scope.$apply();
            }

            const sortable = Sortable.create(gridLayoutContainerEl, {
                group: "uniqueGridEditorID",  // links groups with same name.
                sort: true,  // sorting inside list
                //delay: 0, // time in milliseconds to define when the sorting should start
                //delayOnTouchOnly: false, // only delay if user is using touch
                //touchStartThreshold: 0, // px, how many pixels the point should move before cancelling a delayed drag event
                //disabled: false, // Disables the sortable if set to true.
                //store: null,  // @see Store
                animation: 150,  // ms, animation speed moving items when sorting, `0` — without animation
                easing: "cubic-bezier(1, 0, 0, 1)", // Easing for animation. Defaults to null. See https://easings.net/ for examples.
                //handle: "umb-block-grid-block",  // Drag handle selector within list items,
                cancel: '',
                //filter: ".ignore-elements",  // Selectors that do not lead to dragging (String or Function)
                //preventOnFilter: true, // Call `event.preventDefault()` when triggered `filter`
                draggable: ".umb-block-grid__layout-item",  // Specifies which items inside the element should be draggable

                //dataIdAttr: 'data-element-udi', // HTML attribute that is used by the `toArray()` method

                //ghostClass: "umb-block-grid__layout-item-placeholder umb-block-grid__layout-item",  // Class name for the drop placeholder
                //chosenClass: "sortable-chosen",  // Class name for the chosen item
                //dragClass: "sortable-drag",  // Class name for the dragging item

                //swapThreshold: 1, // Threshold of the swap zone
                //invertSwap: false, // Will always use inverted swap zone if set to true
                //invertedSwapThreshold: 1, // Threshold of the inverted swap zone (will be set to swapThreshold value by default)
                //direction: 'horizontal', // Direction of Sortable (will be detected automatically if not given)

                //forceFallback: false,  // ignore the HTML5 DnD behaviour and force the fallback to kick in

                //fallbackClass: "sortable-fallback",  // Class name for the cloned DOM Element when using forceFallback
                //fallbackOnBody: false,  // Appends the cloned DOM Element into the Document's Body
                //fallbackTolerance: 0, // Specify in pixels how far the mouse should move before it's considered as a drag.

                //dragoverBubble: false,
                //removeCloneOnHide: true, // Remove the clone element when it is not showing, rather than just hiding it
                //emptyInsertThreshold: 5, // px, distance mouse must be from empty sortable to insert drag element into it

                onStart: function (/**Event*/evt) {
                    nextSibling = evt.from === evt.item.parentNode ? evt.item.nextSibling : evt.clone.nextSibling;
                    $scope.$apply();
                },
                onAdd: function (evt) {
                    _sync(evt);
                    $scope.$apply();
                },
                onUpdate: function (evt) {
                    _sync(evt);
                }
                /*
                setData: function (dataTransfer, dragEl) {
                    dataTransfer.setData('Text', dragEl.textContent); // `dataTransfer` object of HTML5 DragEvent
                },

                // Element is chosen
                onChoose: function (evt) {
                    evt.oldIndex;  // element index within parent
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

                // Changed sorting within list
                onUpdate: function (evt) {
                    // same properties as onEnd
                },

                // Called by any change to the list (add / update / remove)
                onSort: function (evt) {
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

            console.log("sortable:", sortable)

        }
    }   

})();
