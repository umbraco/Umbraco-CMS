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

    function ListViewLayoutsPreValsController($scope, editorService, localizationService, overlayService) {

        var vm = this;
        vm.focusLayoutName = false;

        vm.layoutsSortableOptions = {
            axis: "y",
            containment: "parent",
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

       function removeLayout(template, index, event) {

           const dialog = {
               view: "views/propertyEditors/listview/overlays/removeListViewLayout.html",
               layout: template,
               submitButtonLabelKey: "defaultdialogs_yesRemove",
               submitButtonStyle: "danger",
               submit: function (model) {
                   $scope.model.value.splice(index, 1);
                   overlayService.close();
               },
               close: function () {
                   overlayService.close();
               }
           };

           localizationService.localize("general_remove").then(value => {
               dialog.title = value;
               overlayService.open(dialog);
           });

           event.preventDefault();
           event.stopPropagation();
       }

        function openIconPicker(layout) {
            var iconPicker = {
                icon: layout.icon.split(' ')[0],
                color: layout.icon.split(' ')[1],
                size: "medium",
                submit: function (model) {
                    if (model.icon) {
                        if (model.color) {
                            layout.icon = model.icon + " " + model.color;
                        } else {
                            layout.icon = model.icon;
                        }
                    }
                    vm.focusLayoutName = true;
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };
            editorService.iconPicker(iconPicker);
        }

    }

   angular.module("umbraco").controller("Umbraco.PrevalueEditors.ListViewLayoutsPreValsController", ListViewLayoutsPreValsController);

})();
