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

   function TableDirective(iconHelper) {

      function link(scope, el, attr, ctrl) {

         scope.clickItem = function (item, $event) {
            if (scope.onClick) {
               scope.onClick(item);
               $event.stopPropagation();
            }
         };

         scope.selectItem = function (item, $index, $event) {
            if (scope.onSelect) {
               scope.onSelect(item, $index, $event);
               $event.stopPropagation();
            }
         };

         scope.selectAll = function ($event) {
            if (scope.onSelectAll) {
               scope.onSelectAll($event);
            }
         };

         scope.isSelectedAll = function () {
            if (scope.onSelectedAll && scope.items && scope.items.length > 0) {
               return scope.onSelectedAll();
            }
         };

         scope.isSortDirection = function (col, direction) {
            if (scope.onSortingDirection) {
               return scope.onSortingDirection(col, direction);
            }
         };

         scope.sort = function (field, allow, isSystem) {
            if (scope.onSort) {
               scope.onSort(field, allow, isSystem);
            }
         };

         scope.getIcon = function (entry) {
             return iconHelper.convertFromLegacyIcon(entry.icon);
         };

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/umb-table.html',
         scope: {
            items: '=',
            itemProperties: '=',
            allowSelectAll: '=',
            onSelect: '=',
            onClick: '=',
            onSelectAll: '=',
            onSelectedAll: '=',
            onSortingDirection: '=',
            onSort: '='
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbTable', TableDirective);

})();
