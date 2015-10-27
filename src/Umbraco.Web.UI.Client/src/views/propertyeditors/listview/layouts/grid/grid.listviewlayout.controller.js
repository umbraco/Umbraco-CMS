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
      vm.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
      vm.activeDrag = false;
      vm.mediaDetailsTooltip = {};

      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
		vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;
      vm.hoverMediaItemDetails = hoverMediaItemDetails;
      vm.selectItem = selectItem;
      vm.selectFolder = selectFolder;
      vm.clickItem = clickItem;

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

      function selectItem(selectedItem, $event, index) {
         listViewHelper.selectHandler(selectedItem, index, $scope.items, $scope.selection, $event);
      }

      function selectFolder(selectedItem, $event, index) {
         listViewHelper.selectHandler(selectedItem, index, $scope.folders, $scope.selection, $event);
      }

      function clickItem(item) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + item.id);
      }

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
