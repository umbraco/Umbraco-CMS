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

   function ListViewGridLayoutController($scope, $routeParams, mediaHelper, mediaResource, $location) {

      var vm = this;

      //pass in the content id from the grid view parent scope (badbadnotgood)
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

			// call reload function on list view parent controller
         $scope.reloadView($scope.contentId);

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

      function clickItem(item) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + item.id);
      }

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
