/**
@ngdoc directive
@name umbraco.directives.directive:umbTable
@restrict E
@scope

@description
<strong>Added in Umbraco v. 7.4:</strong> Use this directive to render a data table.

<h3>Markup example</h3>
<pre>
    <div ng-controller="My.TableController as vm">
        
        <umb-table
            ng-if="items"
            items="vm.items"
            item-properties="vm.options.includeProperties"
            allow-select-all="vm.allowSelectAll"
            on-select="vm.selectItem"
            on-click="vm.clickItem"
            on-select-all="vm.selectAll"
            on-selected-all="vm.isSelectedAll"
            on-sorting-direction="vm.isSortDirection"
            on-sort="vm.sort">
        </umb-table>
    
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";
    
        function Controller() {
    
            var vm = this;
    
            vm.items = [
                {
                    "icon": "icon-document",
                    "name": "My node 1",
                    "published": true,
                    "description": "A short description of my node",
                    "author": "Author 1"
                },
                {
                    "icon": "icon-document",
                    "name": "My node 2",
                    "published": true,
                    "description": "A short description of my node",
                    "author": "Author 2"
                }
            ];

            vm.options = {
                includeProperties: [
                    { alias: "description", header: "Description" },
                    { alias: "author", header: "Author" }
                ]
            };
    
            vm.selectItem = selectItem;
            vm.clickItem = clickItem;
            vm.selectAll = selectAll;
            vm.isSelectedAll = isSelectedAll;
            vm.isSortDirection = isSortDirection;
            vm.sort = sort;

            function selectAll($event) {
                alert("select all");
            }

            function isSelectedAll() {
                
            }
    
            function clickItem(item) {
                alert("click node");
            }

            function selectItem(selectedItem, $index, $event) {
                alert("select node");
            }
            
            function isSortDirection(col, direction) {
                
            }
            
            function sort(field, allow, isSystem) {
                
            }
    
        }
    
        angular.module("umbraco").controller("My.TableController", Controller);
    
    })();
</pre>

@param {string} icon (<code>binding</code>): The node icon.
@param {string} name (<code>binding</code>): The node name.
@param {string} published (<code>binding</code>): The node published state.
@param {function} onSelect (<code>expression</code>): Callback function when the row is selected.
@param {function} onClick (<code>expression</code>): Callback function when the "Name" column link is clicked.
@param {function} onSelectAll (<code>expression</code>): Callback function when selecting all items.
@param {function} onSelectedAll (<code>expression</code>): Callback function when all items are selected.
@param {function} onSortingDirection (<code>expression</code>): Callback function when sorting direction is changed.
@param {function} onSort (<code>expression</code>): Callback function when sorting items.
**/

(function () {
    'use strict';

    function TableController(iconHelper) {

        var vm = this;

        vm.clickItem = function (item, $event) {
            if (vm.onClick) {
                vm.onClick({ item: item});
                $event.stopPropagation();
            }
        };

        vm.getItemUrl = function (item) {
            if (vm.getItemUrl) {
                vm.getItemUrl({ item: item});
            }
        };

        vm.selectItem = function (item, $index, $event) {
            if (vm.onSelect) {
                vm.onSelect({ item: item, $index: $index, $event: $event });
                $event.stopPropagation();
            }
        };

        vm.selectAll = function ($event) {
            if (vm.onSelectAll) {
                vm.onSelectAll({ $event: $event});
            }
        };

        vm.isSelectedAll = function () {
            if (vm.onSelectedAll && vm.items && vm.items.length > 0) {
                return vm.onSelectedAll();
            }
        };

        vm.isSortDirection = function (col, direction) {
            if (vm.onSortingDirection) {
                return vm.onSortingDirection({ col: col, direction: direction });
            }
        };

        vm.sort = function (field, allow, isSystem) {
            if (vm.onSort) {
                vm.onSort({ field: field, allow: allow, isSystem: isSystem  });
            }
        };

        vm.getIcon = function (entry) {
            return iconHelper.convertFromLegacyIcon(entry.icon);
        };
    }

    angular
        .module('umbraco.directives')
        .component('umbTable', {
            templateUrl: 'views/components/umb-table.html',
            controller: TableController,
            controllerAs: 'vm',
            bindings: {
                items: '<',
                itemProperties: '<',
                allowSelectAll: '<',
                onSelect: '&',
                onClick: '&',
                getItemUrl: '&',
                onSelectAll: '&',
                onSelectedAll: '&',
                onSortingDirection: '&',
                onSort: '&'
            }
        });

})();
