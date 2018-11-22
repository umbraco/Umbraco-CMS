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
      vm.showPrompt = showPrompt;
      vm.hidePrompt = hidePrompt;
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

      function showPrompt(layout) {
          layout.deletePrompt = true;
      }

      function hidePrompt(layout) {
          layout.deletePrompt = false;
      }

      function removeLayout($index, layout) {
         $scope.model.value.splice($index, 1);
      }

      function openIconPicker(layout) {
          vm.iconPickerDialog = {
              view: "iconpicker",
              show: true,
              submit: function(model) {
                  if (model.color) {
                     layout.icon = model.icon + " " + model.color;
                  } else {
                     layout.icon = model.icon;
                  }
                  vm.focusLayoutName = true;
                  vm.iconPickerDialog.show = false;
                  vm.iconPickerDialog = null;
              }
          };
      }

      activate();

   }

   angular.module("umbraco").controller("Umbraco.PrevalueEditors.ListViewLayoutsPreValsController", ListViewLayoutsPreValsController);

})();
