(function() {
   'use strict';

   function listViewHelper() {

      var firstSelectedIndex = 0;

      function selectHandler(selectedItem, selectedIndex, items, selection, $event) {

         var start = 0;
         var end = 0;
         var item = null;

         if ($event.shiftKey === true) {

            if(selectedIndex > firstSelectedIndex) {

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

            if(selectedItem.selected) {
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
         if(!isSelected && !item.hidden) {
            selection.push({id: item.id});
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

         if(angular.isArray(items)) {
            for(i = 0; items.length > i; i++) {
               var item = items[i];
               item.selected = false;
            }
         }

         if(angular.isArray(items)) {
            for(i = 0; folders.length > i; i++) {
               var folder = folders[i];
               folder.selected = false;
            }
         }
      }

      var service = {
         selectHandler: selectHandler,
         selectItem: selectItem,
         deselectItem: deselectItem,
         clearSelection: clearSelection
      };

      return service;

   }


   angular.module('umbraco.services').factory('listViewHelper', listViewHelper);


})();
