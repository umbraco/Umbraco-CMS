(function() {
   'use strict';

   function EditorViewDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-view.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorView', EditorViewDirective);

})();
