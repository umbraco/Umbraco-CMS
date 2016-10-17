/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.EditController
 * @function
 *
 * @description
 * The controller for the content type editor
 */
(function() {
   "use strict";

   function ListViewGridLayoutController($scope, $routeParams, mediaHelper, mediaResource, $location, listViewHelper) {

      var vm = this;

      vm.nodeId = $scope.contentId;
      //we pass in a blacklist by adding ! to the file extensions, allowing everything EXCEPT for disallowedUploadFiles
      vm.acceptedFileTypes = !mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.disallowedUploadFiles);
      vm.maxFileSize = Umbraco.Sys.ServerVariables.umbracoSettings.maxFileSize + "KB";
      vm.activeDrag = false;
      vm.mediaDetailsTooltip = {};
      vm.itemsWithoutFolders = [];
      vm.isRecycleBin = $scope.contentId === '-21' || $scope.contentId === '-20';

      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
	  vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;

      vm.hoverMediaItemDetails = hoverMediaItemDetails;
      vm.selectContentItem = selectContentItem;
      vm.selectItem = selectItem;
      vm.selectFolder = selectFolder;
      vm.goToItem = goToItem;

      function activate() {
          vm.itemsWithoutFolders = filterOutFolders($scope.items);
      }

      function filterOutFolders(items) {

          var newArray = [];

          if(items && items.length ) {

              for (var i = 0; items.length > i; i++) {
                  var item = items[i];
                  var isFolder = !mediaHelper.hasFilePropertyType(item);

                  if (!isFolder) {
                      newArray.push(item);
                  }
              }

          }

          return newArray;
      }

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

      function hoverMediaItemDetails(item, event, hover) {

         if (hover && !vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.event = event;
            vm.mediaDetailsTooltip.item = item;
            vm.mediaDetailsTooltip.show = true;

         } else if (!hover && vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.show = false;

         }

      }

      function selectContentItem(item, $event, $index) {
          listViewHelper.selectHandler(item, $index, $scope.items, $scope.selection, $event);
      }

      function selectItem(item, $event, $index) {
          listViewHelper.selectHandler(item, $index, vm.itemsWithoutFolders, $scope.selection, $event);
      }

      function selectFolder(folder, $event, $index) {
          listViewHelper.selectHandler(folder, $index, $scope.folders, $scope.selection, $event);
      }

      function goToItem(item, $event, $index) {
          $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + item.id);
      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
