(function() {
   'use strict';

   function EditorViewDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-view.html',
         scope: {
             footer: "@"
         }
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorView', EditorViewDirective);

})();
