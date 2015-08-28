(function() {
   'use strict';

   function EditorMenuDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/navigation/umb-editor-menu.html',
         scope: {
            menu: "=",
            dimmed: "="
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorMenu', EditorMenuDirective);

})();
