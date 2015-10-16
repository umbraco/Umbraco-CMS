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
      vm.nodeId = $routeParams.id;
      vm.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
      vm.activeDrag = false;

      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
		vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;
      vm.showMediaDetailsTooltip = showMediaDetailsTooltip;
      vm.selectFolder = selectFolder;
      vm.clickFolder = clickFolder;

      function activate() {

         mediaResource.getChildFolders(vm.nodeId)
            .then(function(folders) {
               vm.folders = folders;
            });
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


      function showMediaDetailsTooltip(item, event, isHovering) {

         if (isHovering && !vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.event = event;
            vm.mediaDetailsTooltip.item = item;
            vm.mediaDetailsTooltip.show = true;

         } else if (!isHovering && vm.mediaDetailsTooltip.show) {

            vm.mediaDetailsTooltip.show = false;

         }

      }

      function selectFolder(folder) {
         folder.selected = !folder.selected;
      }

      function clickFolder(folder) {
         $location.path($scope.entityType + '/' + $scope.entityType + '/edit/' + folder.id);
      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
