(function() {
   'use strict';

   function EditorSubViewsDirective() {

      function link(scope, el, attr, ctrl) {

         scope.activeView = {};

         // set toolbar from selected navigation item
         function setActiveView(items) {

            for (var index = 0; index < items.length; index++) {

               var item = items[index];

               if (item.active && item.view) {
                  scope.activeView = item;
               }
            }
         }

         // watch for navigation changes
         scope.$watch('subViews', function(newValue, oldValue) {
            if (newValue) {
               setActiveView(newValue);
            }
         }, true);

      }

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-sub-views.html',
         scope: {
            subViews: "=",
            model: "="
         },
         link: link
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorSubViews', EditorSubViewsDirective);

})();
