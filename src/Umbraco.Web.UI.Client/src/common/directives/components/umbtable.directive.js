(function () {
   'use strict';

   function TableDirective() {

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
