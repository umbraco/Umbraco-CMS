/**
 @ngdoc service
 * @name umbraco.services.listViewHelper
 *
 *
 * @description
 * Service for performing operations against items in the list view UI. Used by the built-in internal listviews
 * as well as custom listview.
 *
 * A custom listview is always used inside a wrapper listview, so there are a number of inherited values on its
 * scope by default:
 *
 * **$scope.selection**: Array containing all items currently selected in the listview
 *
 * **$scope.items**: Array containing all items currently displayed in the listview
 *
 * **$scope.folders**: Array containing all folders in the current listview (only for media)
 *
 * **$scope.options**: configuration object containing information such as pagesize, permissions, order direction etc.
 *
 * **$scope.model.config.layouts**: array of available layouts to apply to the listview (grid, list or custom layout)
 *
 * ##Usage##
 * To use, inject listViewHelper into custom listview controller, listviewhelper expects you
 * to pass in the full collection of items in the listview in several of its methods
 * this collection is inherited from the parent controller and is available on $scope.selection
 *
 * <pre>
 *      angular.module("umbraco").controller("my.listVieweditor". function($scope, listViewHelper){
 *
 *          //current items in the listview
 *          var items = $scope.items;
 *
 *          //current selection
 *          var selection = $scope.selection;
 *
 *          //deselect an item , $scope.selection is inherited, item is picked from inherited $scope.items
 *          listViewHelper.deselectItem(item, $scope.selection);
 *
 *          //test if all items are selected, $scope.items + $scope.selection are inherited
 *          listViewhelper.isSelectedAll($scope.items, $scope.selection);
 *      });
 * </pre>
 */
(function () {
    'use strict';

    function listViewHelper($location, $rootScope, localStorageService, urlHelper, editorService) {
        var firstSelectedIndex = 0;
        var localStorageKey = "umblistViewLayout";

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#getLayout
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Method for internal use, based on the collection of layouts passed, the method selects either
        * any previous layout from local storage, or picks the first allowed layout
        *
        * @param {Any} id The identifier of the current node or application displayed in the content editor
        * @param {Array} availableLayouts Array of all allowed layouts, available from $scope.model.config.layouts
        */

        function getLayout(id, availableLayouts) {

            var storedLayouts = [];

            if (localStorageService.get(localStorageKey)) {
                storedLayouts = localStorageService.get(localStorageKey);
            }

            if (storedLayouts && storedLayouts.length > 0) {
                for (var i = 0; storedLayouts.length > i; i++) {
                    var layout = storedLayouts[i];
                    if (isMatchingLayout(id, layout)) {
                        return setLayout(id, layout, availableLayouts);
                    }
                }

            }

            return getFirstAllowedLayout(availableLayouts);

        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#setLayout
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Changes the current layout used by the listview to the layout passed in. Stores selection in localstorage
        *
        * @param {Any} id The identifier of the current node or application displayed in the content editor
        * @param {Object} selectedLayout Layout selected as the layout to set as the current layout
        * @param {Array} availableLayouts Array of all allowed layouts, available from $scope.model.config.layouts
        */

        function setLayout(id, selectedLayout, availableLayouts) {

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

            saveLayoutInLocalStorage(id, activeLayout);

            return activeLayout;

        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#saveLayoutInLocalStorage
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Stores a given layout as the current default selection in local storage
        *
        * @param {Any} id The identifier of the current node or application displayed in the content editor
        * @param {Object} selectedLayout Layout selected as the layout to set as the current layout
        */

        function saveLayoutInLocalStorage(id, selectedLayout) {
            var layoutFound = false;
            var storedLayouts = [];

            if (localStorageService.get(localStorageKey)) {
                storedLayouts = localStorageService.get(localStorageKey);
            }

            if (storedLayouts.length > 0) {
                for (var i = 0; storedLayouts.length > i; i++) {
                    var layout = storedLayouts[i];
                    if (isMatchingLayout(id, layout)) {
                        layout.path = selectedLayout.path;
                        layoutFound = true;
                    }
                }
            }

            if (!layoutFound) {
                var storageObject = {
                    "id": id,
                    "path": selectedLayout.path
                };
                storedLayouts.push(storageObject);
            }

            localStorageService.set(localStorageKey, storedLayouts);

        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#getFirstAllowedLayout
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Returns currently selected layout, or alternatively the first layout in the available layouts collection
        *
        * @param {Array} layouts Array of all allowed layouts, available from $scope.model.config.layouts
        */

        function getFirstAllowedLayout(layouts) {

            var firstAllowedLayout = {};

            if (layouts != null) {
                for (var i = 0; layouts.length > i; i++) {
                    var layout = layouts[i];
                    if (layout.selected === true) {
                        firstAllowedLayout = layout;
                        break;
                    }
                }
            }

            return firstAllowedLayout;
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#selectHandler
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Helper method for working with item selection via a checkbox, internally it uses selectItem and deselectItem.
        * Working with this method, requires its triggered via a checkbox which can then pass in its triggered $event
        * When the checkbox is clicked, this method will toggle selection of the associated item so it matches the state of the checkbox
        *
        * @param {Object} selectedItem Item being selected or deselected by the checkbox
        * @param {Number} selectedIndex Index of item being selected/deselected, usually passed as $index
        * @param {Array} items All items in the current listview, available as $scope.items
        * @param {Array} selection All selected items in the current listview, available as $scope.selection
        * @param {Event} $event Event triggered by the checkbox being checked to select / deselect an item
        */

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

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#selectItem
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Selects a given item to the listview selection array, requires you pass in the inherited $scope.selection collection
        *
        * @param {Object} item Item to select
        * @param {Array} selection Listview selection, available as $scope.selection
        */

        function selectItem(item, selection) {
            var isSelected = false;
            for (var i = 0; selection.length > i; i++) {
                var selectedItem = selection[i];
                // if item.id is 2147483647 (int.MaxValue) use item.key
                if ((item.id !== 2147483647 && item.id === selectedItem.id) || (item.key && item.key === selectedItem.key)) {
                    isSelected = true;
                }
            }
            if (!isSelected) {
                var obj = {
                    id: item.id
                };
                if (item.key) {
                    obj.key = item.key;
                }

                selection.push(obj);
                item.selected = true;
                $rootScope.$broadcast("listView.itemsChanged", { items: selection });
            }
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#deselectItem
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Deselects a given item from the listviews selection array, requires you pass in the inherited $scope.selection collection
        *
        * @param {Object} item Item to deselect
        * @param {Array} selection Listview selection, available as $scope.selection
        */

        function deselectItem(item, selection) {
            for (var i = 0; selection.length > i; i++) {
                var selectedItem = selection[i];
                // if item.id is 2147483647 (int.MaxValue) use item.key
                if ((item.id !== 2147483647 && item.id === selectedItem.id) || (item.key && item.key === selectedItem.key)) {
                    selection.splice(i, 1);
                    item.selected = false;
                    $rootScope.$broadcast("listView.itemsChanged", { items: selection });
                }
            }
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#clearSelection
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Removes a given number of items and folders from the listviews selection array
        * Folders can only be passed in if the listview is used in the media section which has a concept of folders.
        *
        * @param {Array} items Items to remove, can be null
        * @param {Array} folders Folders to remove, can be null
        * @param {Array} selection Listview selection, available as $scope.selection
        */

        function clearSelection(items, folders, selection) {

            var i = 0;

            selection.length = 0;

            if (Utilities.isArray(items)) {
                for (i = 0; items.length > i; i++) {
                    var item = items[i];
                    item.selected = false;
                }
            }

            if (Utilities.isArray(folders)) {
                for (i = 0; folders.length > i; i++) {
                    var folder = folders[i];
                    folder.selected = false;
                }
            }
            $rootScope.$broadcast("listView.itemsChanged", { items: selection });
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#selectAllItems
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Helper method for toggling the select state on all items in the active listview
        * Can only be used from a checkbox as a checkbox $event is required to pass in.
        *
        * @param {Array} items Items to toggle selection on, should be $scope.items
        * @param {Array} selection Listview selection, available as $scope.selection
        * @param {$event} $event Event passed from the checkbox being toggled
        */

        function selectAllItems(items, selection, $event) {

            var checkbox = $event.target;
            var clearSelection = false;

            if (!Utilities.isArray(items)) {
                return;
            }

            selection.length = 0;

            for (var i = 0; i < items.length; i++) {

                var item = items[i];
                var obj = {
                    id: item.id
                };
                if (item.key) {
                    obj.key = item.key
                }

                if (checkbox.checked) {
                    selection.push(obj);
                } else {
                    clearSelection = true;
                }

                item.selected = checkbox.checked;

            }

            if (clearSelection) {
                selection.length = 0;
            }
            $rootScope.$broadcast("listView.itemsChanged", { items: selection });

        }


        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#selectAllItemsToggle
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Helper method for toggling the select state on all items.
        *
        * @param {Array} items Items to toggle selection on, should be $scope.items
        * @param {Array} selection Listview selection, available as $scope.selection
        */

        function selectAllItemsToggle(items, selection) {

            if (!Utilities.isArray(items)) {
                return;
            }

            if (isSelectedAll(items, selection)) {
                // unselect all items
                items.forEach(function (item) {
                    item.selected = false;
                });

                // reset selection without loosing reference.
                selection.length = 0;

            } else {

                // reset selection without loosing reference.
                selection.length = 0;

                // select all items
                items.forEach(function (item) {
                    var obj = {
                        id: item.id
                    };
                    if (item.key) {
                        obj.key = item.key;
                    }
                    item.selected = true;
                    selection.push(obj);
                });
            }
            $rootScope.$broadcast("listView.itemsChanged", { items: selection });

        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#isSelectedAll
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Method to determine if all items on the current page in the list has been selected
        * Given the current items in the view, and the current selection, it will return true/false
        *
        * @param {Array} items Items to test if all are selected, should be $scope.items
        * @param {Array} selection Listview selection, available as $scope.selection
        * @returns {Boolean} boolean indicate if all items in the listview have been selected
        */

        function isSelectedAll(items, selection) {

            var numberOfSelectedItem = 0;

            for (var itemIndex = 0; items.length > itemIndex; itemIndex++) {
                var item = items[itemIndex];

                for (var selectedIndex = 0; selection.length > selectedIndex; selectedIndex++) {
                    var selectedItem = selection[selectedIndex];

                    // if item.id is 2147483647 (int.MaxValue) use item.key
                    if ((item.id !== 2147483647 && item.id === selectedItem.id) || (item.key && item.key === selectedItem.key)) {
                        numberOfSelectedItem++;
                    }
                }

            }

            if (numberOfSelectedItem === items.length) {
                return true;
            }

        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#setSortingDirection
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * *Internal* method for changing sort order icon
        * @param {String} col Column alias to order after
        * @param {String} direction Order direction `asc` or `desc`
        * @param {Object} options object passed from the parent listview available as $scope.options
        */

        function setSortingDirection(col, direction, options) {
            return options.orderBy.toUpperCase() === col.toUpperCase() && options.orderDirection === direction;
        }

        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#setSorting
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Method for setting the field on which the listview will order its items after.
        *
        * @param {String} field Field alias to order after
        * @param {Boolean} allow Determines if the user is allowed to set this field, normally true
        * @param {Object} options Options object passed from the parent listview available as $scope.options
        */

        function setSorting(field, allow, options) {
            if (allow) {
                if (options.orderBy === field && options.orderDirection === 'asc') {
                    options.orderDirection = "desc";
                } else {
                    options.orderDirection = "asc";
                }
                options.orderBy = field;
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
            //see: https://stackoverflow.com/a/16229480/694494
            var intersectPermissions = _.intersection.apply(_, arr);

            return {
                canCopy: _.contains(intersectPermissions, 'O'), //Magic Char = O
                canCreate: _.contains(intersectPermissions, 'C'), //Magic Char = C
                canDelete: _.contains(intersectPermissions, 'D'), //Magic Char = D
                canMove: _.contains(intersectPermissions, 'M'), //Magic Char = M
                canPublish: _.contains(intersectPermissions, 'U'), //Magic Char = U
                canUnpublish: _.contains(intersectPermissions, 'U') //Magic Char = Z (however UI says it can't be set, so if we can publish 'U' we can unpublish)
            };
        }


        /**
        * @ngdoc method
        * @name umbraco.services.listViewHelper#editItem
        * @methodOf umbraco.services.listViewHelper
        *
        * @description
        * Method for opening an item in a list view for editing.
        *
        * @param {Object} item The item to edit
        * @param {Object} scope The scope with options
        */
        function editItem(item, scope) {

            if (!item.editPath) {
                return;
            }

            if (scope && scope.options && scope.options.useInfiniteEditor)
            {
                var editorModel = {
                    id: item.id,
                    submit: function(model) {
                        editorService.close();
                        scope.getContent(scope.contentId);
                    },
                    close: function() {
                        editorService.close();
                        scope.getContent(scope.contentId);
                    }
                };

                if (item.editPath.indexOf("/content/") == 0)
                {
                    editorService.contentEditor(editorModel);
                    return;
                }

                if (item.editPath.indexOf("/media/") == 0)
                {
                    editorService.mediaEditor(editorModel);
                    return;
                }

                if (item.editPath.indexOf("/member/") == 0)
                {
                    editorModel.id = item.key;
                    editorService.memberEditor(editorModel);
                    return;
                }
            }
            
            var parts = item.editPath.split("?");
            var path = parts[0];
            var params = parts[1]
            ? urlHelper.getQueryStringParams("?" + parts[1])
            : {};
            
            $location.path(path);
            for (var p in params) {
                $location.search(p, params[p]);
            }
        }

        function isMatchingLayout(id, layout) {
            // legacy format uses "nodeId", be sure to look for both
            return layout.id === id || layout.nodeId === id;
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
            selectAllItemsToggle: selectAllItemsToggle,
            isSelectedAll: isSelectedAll,
            setSortingDirection: setSortingDirection,
            setSorting: setSorting,
            getButtonPermissions: getButtonPermissions,
            editItem: editItem

        };

        return service;

    }


    angular.module('umbraco.services').factory('listViewHelper', listViewHelper);


})();
