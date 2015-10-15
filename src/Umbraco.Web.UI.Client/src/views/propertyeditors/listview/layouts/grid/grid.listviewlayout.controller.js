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

   function ListViewGridLayoutController($scope, $routeParams, mediaHelper) {

      var vm = this;

      vm.nodeId = $routeParams.id;
      vm.acceptedFileTypes = mediaHelper.formatFileTypes(Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes);
      vm.activeDrag = false;

      vm.dragEnter = dragEnter;
      vm.dragLeave = dragLeave;
		vm.onFilesQueue = onFilesQueue;
      vm.onUploadComplete = onUploadComplete;
      vm.showMediaDetailsTooltip = showMediaDetailsTooltip;

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

   }

   angular.module("umbraco").controller("Umbraco.PropertyEditors.ListView.GridLayoutController", ListViewGridLayoutController);

})();
