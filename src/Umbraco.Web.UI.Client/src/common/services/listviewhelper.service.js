(function () {
    'use strict';

    function listViewHelper(localStorageService) {

        var firstSelectedIndex = 0;
        var localStorageKey = "umblistViewLayout";

        function getLayout(nodeId, availableLayouts) {

            var storedLayouts = [];

            if (localStorageService.get(localStorageKey)) {
                storedLayouts = localStorageService.get(localStorageKey);
            }

            if (storedLayouts && storedLayouts.length > 0) {
                for (var i = 0; storedLayouts.length > i; i++) {
                    var layout = storedLayouts[i];
                    if (layout.nodeId === nodeId) {
                        return setLayout(nodeId, layout, availableLayouts);
                    }
                }

            }

            return getFirstAllowedLayout(availableLayouts);

        }

        function setLayout(nodeId, selectedLayout, availableLayouts) {

            var activeLayout = {};
            var layoutFound = false;

            for (var i = 0; availableLayouts.length > i; i++) {
                var layout = availableLayouts[i];
                if (layout.path === selectedLayout.path) {
                    activeLayout = layout;
                    layout.active = true;
                    layoutFound = true;
                } else {
                    layout.active = false;
                }
            }

            if (!layoutFound) {
                activeLayout = getFirstAllowedLayout(availableLayouts);
            }

            saveLayoutInLocalStorage(nodeId, activeLayout);

            return activeLayout;

        }

        function saveLayoutInLocalStorage(nodeId, selectedLayout) {
            var layoutFound = false;
            var storedLayouts = [];

            if (localStorageService.get(localStorageKey)) {
                storedLayouts = localStorageService.get(localStorageKey);
            }

            if (storedLayouts.length > 0) {
                for (var i = 0; storedLayouts.length > i; i++) {
                    var layout = storedLayouts[i];
                    if (layout.nodeId === nodeId) {
                        layout.path = selectedLayout.path;
                        layoutFound = true;
                    }
                }
            }

            if (!layoutFound) {
                var storageObject = {
                    "nodeId": nodeId,
                    "path": selectedLayout.path
                };
                storedLayouts.push(storageObject);
            }

            localStorageService.set(localStorageKey, storedLayouts);

        }

        function getFirstAllowedLayout(layouts) {

            var firstAllowedLayout = {};

            for (var i = 0; layouts.length > i; i++) {
                var layout = layouts[i];
                if (layout.selected === true) {
                    firstAllowedLayout = layout;
                    break;
                }
            }

            return firstAllowedLayout;
        }

        function selectHandler(selectedItem, selectedIndex, items, selection, $event) {

            var start = 0;
            var end = 0;
            var item = null;

            if ($event.shiftKey === true) {

                if (selectedIndex > firstSelectedIndex) {

                    start = firstSelectedIndex;
                    end = selectedIndex;

                    for (; end >= start; start++) {
                        item = items[start];
                        selectItem(item, selection);
                    }

                } else {

                    start = firstSelectedIndex;
                    end = selectedIndex;

                    for (; end <= start; start--) {
                        item = items[start];
                        selectItem(item, selection);
                    }

                }

            } else {

                if (selectedItem.selected) {
                    deselectItem(selectedItem, selection);
                } else {
                    selectItem(selectedItem, selection);
                }

                firstSelectedIndex = selectedIndex;

            }

        }

        function selectItem(item, selection) {
            var isSelected = false;
            for (var i = 0; selection.length > i; i++) {
                var selectedItem = selection[i];
                if (item.id === selectedItem.id) {
                    isSelected = true;
                }
            }
            if (!isSelected) {
                selection.push({ id: item.id });
                item.selected = true;
            }
        }

        function deselectItem(item, selection) {
            for (var i = 0; selection.length > i; i++) {
                var selectedItem = selection[i];
                if (item.id === selectedItem.id) {
                    selection.splice(i, 1);
                    item.selected = false;
                }
            }
        }

        function clearSelection(items, folders, selection) {

            var i = 0;

            selection.length = 0;

            if (angular.isArray(items)) {
                for (i = 0; items.length > i; i++) {
                    var item = items[i];
                    item.selected = false;
                }
            }

            if (angular.isArray(items)) {
                for (i = 0; folders.length > i; i++) {
                    var folder = folders[i];
                    folder.selected = false;
                }
            }
        }

        function selectAllItems(items, selection, $event) {

            var checkbox = $event.target;
            var clearSelection = false;

            if (!angular.isArray(items)) {
                return;
            }

            selection.length = 0;

            for (var i = 0; i < items.length; i++) {

                var item = items[i];

                if (checkbox.checked) {
                    selection.push({ id: item.id });
                } else {
                    clearSelection = true;
                }

                item.selected = checkbox.checked;

            }

            if (clearSelection) {
                selection.length = 0;
            }

        }

        function isSelectedAll(items, selection) {

            var numberOfSelectedItem = 0;

            for (var itemIndex = 0; items.length > itemIndex; itemIndex++) {
                var item = items[itemIndex];

                for (var selectedIndex = 0; selection.length > selectedIndex; selectedIndex++) {
                    var selectedItem = selection[selectedIndex];

                    if (item.id === selectedItem.id) {
                        numberOfSelectedItem++;
                    }
                }

            }

            if (numberOfSelectedItem === items.length) {
                return true;
            }

        }

        function setSortingDirection(col, direction, options) {
            return options.orderBy.toUpperCase() === col.toUpperCase() && options.orderDirection === direction;
        }

        function setSorting(field, allow, options) {
            if (allow) {
                options.orderBy = field;

                if (options.orderDirection === "desc") {
                    options.orderDirection = "asc";
                } else {
                    options.orderDirection = "desc";
                }
            }
        }
        
        //This takes in a dictionary of Ids with Permissions and determines
        // the intersect of all permissions to return an object representing the
        // listview button permissions
        function getButtonPermissions(unmergedPermissions, currentIdsWithPermissions) {
            
            if (currentIdsWithPermissions == null) {
                currentIdsWithPermissions = {};
            }

            //merge the newly retrieved permissions to the main dictionary
            _.each(unmergedPermissions, function (value, key, list) {
                currentIdsWithPermissions[key] = value;
            });

            //get the intersect permissions
            var arr = [];
            _.each(currentIdsWithPermissions, function (value, key, list) {
                arr.push(value);
            });

            //we need to use 'apply' to call intersection with an array of arrays,
            //see: http://stackoverflow.com/a/16229480/694494
            var intersectPermissions = _.intersection.apply(_, arr);

            return {
                canCopy: _.contains(intersectPermissions, 'O'), //Magic Char = O
                canCreate: _.contains(intersectPermissions, 'C'), //Magic Char = C
                canDelete: _.contains(intersectPermissions, 'D'), //Magic Char = D
                canMove: _.contains(intersectPermissions, 'M'), //Magic Char = M                
                canPublish: _.contains(intersectPermissions, 'U'), //Magic Char = U
                canUnpublish: _.contains(intersectPermissions, 'U'), //Magic Char = Z (however UI says it can't be set, so if we can publish 'U' we can unpublish)                        
            };
        }

        var service = {
            getLayout: getLayout,
            getFirstAllowedLayout: getFirstAllowedLayout,
            setLayout: setLayout,
            saveLayoutInLocalStorage: saveLayoutInLocalStorage,
            selectHandler: selectHandler,
            selectItem: selectItem,
            deselectItem: deselectItem,
            clearSelection: clearSelection,
            selectAllItems: selectAllItems,
            isSelectedAll: isSelectedAll,
            setSortingDirection: setSortingDirection,
            setSorting: setSorting,
            getButtonPermissions: getButtonPermissions
        };

        return service;

    }


    angular.module('umbraco.services').factory('listViewHelper', listViewHelper);


})();
