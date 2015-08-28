(function() {
   'use strict';

   function EditorFooterDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-footer.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorFooter', EditorFooterDirective);

})();
