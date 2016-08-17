(function () {
   "use strict";

   function ListViewListLayoutController($scope, listViewHelper, $location, mediaHelper) {

      var vm = this;

      vm.nodeId = $scope.contentId;
        //we pass in a blacklist by adding ! to the file extensions, allowing everything EXCEPT for disallowedUploadFiles
        vm.acceptedFileTypes = !mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.disallowedUploadFiles);
      vm.maxFileSize = Umbraco.Sys.ServerVariables.umbracoSettings.maxFileSize + "KB";
      vm.activeDrag = false;
      vm.isRecycleBin = $scope.contentId === '-21' || $scope.contentId === '-20';

      vm.selectItem = selectItem;
      vm.clickItem = clickItem;
      vm.selectAll = selectAll;
      vm.isSelectedAll = isSelectedAll;
      vm.isSortDirection = isSortDirection;
      vm.sort = sort;
      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
      vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;

      function selectAll($event) {
         listViewHelper.selectAllItems($scope.items, $scope.selection, $event);
         }

      function isSelectedAll() {
         return listViewHelper.isSelectedAll($scope.items, $scope.selection);
         }

      function selectItem(selectedItem, $index, $event) {
         listViewHelper.selectHandler(selectedItem, $index, $scope.items, $scope.selection, $event);
         }

      function clickItem(item) {
         $location.path($scope.entityType + '/' +$scope.entityType + '/edit/' +item.id);
         }

      function isSortDirection(col, direction) {
         return listViewHelper.setSortingDirection(col, direction, $scope.options);
         }

      function sort(field, allow, isSystem) {
         if (allow) {
            $scope.options.orderBySystemField = isSystem;
            listViewHelper.setSorting(field, allow, $scope.options);
            $scope.getContent($scope.contentId);
            }
            }

               // Dropzone upload functions
      function dragEnter(el, event) {
         vm.activeDrag = true;
   }

      function dragLeave(el, event) {
         vm.activeDrag = false;
      }

      function onFilesQueue() {
         vm.activeDrag = false;
         }

      function onUploadComplete() { 
         $scope.getContent($scope.contentId);
   }

   }

angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.ListLayoutController", ListViewListLayoutController);

}) ();
