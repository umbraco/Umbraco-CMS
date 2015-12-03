(function() {
   'use strict';

   function EditorFooterContentRightDirective() {

      var directive = {
         transclude: true,
         restrict: 'E',
         replace: true,
         templateUrl: 'views/components/editor/umb-editor-footer-content-right.html'
      };

      return directive;
   }

   angular.module('umbraco.directives').directive('umbEditorFooterContentRight', EditorFooterContentRightDirective);

})();
