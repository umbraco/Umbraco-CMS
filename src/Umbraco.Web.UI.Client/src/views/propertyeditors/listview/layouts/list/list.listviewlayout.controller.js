(function() {
   "use strict";

   function ListViewListLayoutController($scope) {

      var vm = this;

      vm.selectItem = selectItem;
      vm.selectAll = selectAll;
      vm.isSelectedAll = isSelectedAll;
      vm.isSortDirection = isSortDirection;
      vm.sort = sort;

      function selectAll($event) {
         var checkbox = $event.target;
         var clearSelection = false;

         if (!angular.isArray($scope.items)) {
            return;
         }

         $scope.selection.length = 0;

         for (var i = 0; i < $scope.items.length; i++) {

            var entity = $scope.items[i];

            if (checkbox.checked) {
               $scope.selection.push({id: entity.id});
            } else {
               clearSelection = true;
            }

            entity.selected = checkbox.checked;

         }

         if (clearSelection) {
            $scope.selection.length = 0;
         }

      }

      function selectItem(item) {

         var selection = $scope.selection;
         var isSelected = false;

         for (var i = 0; selection.length > i; i++) {
            var selectedItem = selection[i];

            if (item.id === selectedItem.id) {
               isSelected = true;
               selection.splice(i, 1);
               item.selected = false;
            }
         }

         if (!isSelected) {
            selection.push({id: item.id});
            item.selected = true;
         }

      }

      function isSelectedAll() {
         if (!angular.isArray($scope.items)) {
            return false;
         }
         return _.every($scope.items, function(item) {
            return item.selected;
         });
      }

      function isSortDirection(col, direction) {
          return $scope.options.orderBy.toUpperCase() == col.toUpperCase() && $scope.options.orderDirection == direction;
      }

      function sort(field, allow) {
          if (allow) {
             $scope.options.orderBy = field;

             if ($scope.options.orderDirection === "desc") {
                  $scope.options.orderDirection = "asc";
             }
             else {
                  $scope.options.orderDirection = "desc";
             }

             $scope.getContent($scope.contentId);

          }
      }

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.ListLayoutController", ListViewListLayoutController);

})();
