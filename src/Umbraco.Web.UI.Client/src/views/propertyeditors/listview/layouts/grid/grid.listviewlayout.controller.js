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

      vm.folders = [];
      //pass in the content id from the grid view parent scope (badbadnotgood)
      vm.nodeId = $scope.contentId;
      vm.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
      vm.activeDrag = false;

      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
		vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;
      vm.hoverMediaItemDetails = hoverMediaItemDetails;
      vm.selectFolder = selectFolder;
      vm.clickFolder = clickFolder;
      vm.selectMediaItem = selectMediaItem;
      vm.clickMediaItem = clickMediaItem;
      vm.selectContentItem = selectContentItem;
      vm.clickContentItem = clickContentItem;

      function activate() {

         if($scope.entityType === 'media') {
            mediaResource.getChildFolders(vm.nodeId)
               .then(function(folders) {
                  vm.folders = folders;
               });
         }

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

			// call reload function on list view parent controller
         $scope.reloadView($scope.contentId);

      }

      vm.mediaDetailsTooltip = {};


      function hoverMediaItemDetails(item, event, hover) {

         if (hover && !vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.event = event;
            vm.mediaDetailsTooltip.item = item;
            vm.mediaDetailsTooltip.show = true;

         } else if (!hover && vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.show = false;

         }

      }

      function selectFolder(folder) {
         folder.selected = !folder.selected;
      }

      function clickFolder(folder) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + folder.id);
      }

      function selectMediaItem(mediaItem) {
         mediaItem.selected = !mediaItem.selected;
      }

      function clickMediaItem(mediaItem) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + mediaItem.id);
      }

      function selectContentItem(contentItem) {
         contentItem.selected = !contentItem.selected;
      }

      function clickContentItem(contentItem) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + contentItem.id);
      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
