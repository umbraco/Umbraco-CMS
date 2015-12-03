(function() {
   'use strict';

   function EditorHeaderDirective(iconHelper) {

      function link(scope, el, attr, ctrl) {

         scope.openIconPicker = function() {

            scope.dialogModel = {};
            scope.dialogModel.title = "Choose icon";
            scope.dialogModel.view = "iconPicker";
            scope.showDialog = true;

            scope.dialogModel.pickIcon = function(icon, color) {

               if (color) {
                  scope.icon = icon + " " + color;
               } else {
                  scope.icon = icon;
               }

               scope.showDialog = false;
               scope.dialogModel = null;

            };

            scope.dialogModel.close = function() {
               scope.showDialog = false;
               scope.dialogModel = null;
            };

         };

      }

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-header.html',
         scope: {
            tabs: "=",
            actions: "=",
            name: "=",
            nameLocked: "=",
            menu: "=",
            icon: "=",
            hideIcon: "@",
            alias: "=",
            hideAlias: "@",
            description: "=",
            hideDescription: "@",
            navigation: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorHeader', EditorHeaderDirective);

})();
