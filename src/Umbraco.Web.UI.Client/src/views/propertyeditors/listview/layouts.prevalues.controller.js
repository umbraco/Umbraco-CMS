/**
 * @ngdoc controller
 * @name Umbraco.PrevalueEditors.ListViewLayoutsPreValsController
 * @function
 *
 * @description
 * The controller for configuring layouts for list views
 */
(function() {
   "use strict";

   function ListViewLayoutsPreValsController($scope) {

      var vm = this;
      vm.focusLayoutName = false;

      vm.layoutsSortableOptions = {
         distance: 10,
         tolerance: "pointer",
         opacity: 0.7,
         scroll: true,
         cursor: "move",
         handle: ".list-view-layout__sort-handle"
      };

      vm.addLayout = addLayout;
      vm.removeLayout = removeLayout;
      vm.openIconPicker = openIconPicker;

      function activate() {



      }

      function addLayout() {

         vm.focusLayoutName = false;

         var layout = {
            "name": "",
            "path": "",
            "icon": "icon-stop",
            "selected": true
         };

         $scope.model.value.push(layout);

      }

      function removeLayout($index, layout) {
         $scope.model.value.splice($index, 1);
      }

      function openIconPicker(layout) {

         vm.iconPickerDialog = {};
         vm.iconPickerDialog.view = "iconpicker";
         vm.iconPickerDialog.show = true;

         vm.iconPickerDialog.pickIcon = function(icon, color) {

            layout.icon = icon;
            vm.focusLayoutName = true;

            vm.iconPickerDialog.show = false;
            vm.iconPickerDialog = null;
         };


         vm.iconPickerDialog.close = function(oldModel) {

            vm.iconPickerDialog.show = false;
            vm.iconPickerDialog = null;
         };


      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.PrevalueEditors.ListViewLayoutsPreValsController", ListViewLayoutsPreValsController);

})();
