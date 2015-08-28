(function() {
   'use strict';

   function EditorToolbarDirective() {

      var directive = {
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-toolbar.html',
         scope: {
            tools: "="
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorToolbar', EditorToolbarDirective);

})();
